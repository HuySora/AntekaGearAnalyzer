using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AntekaEquipmentAnalyzer {
    public class Gear {
        public int gearType = 1; // 0 is heroic, 1 is epic
        public string gearTypeStr => gearType == 0 ? "Heroic" : "Epic";
        public int eLevel = 0;
        public int rolls => (eLevel / 3) - ((gearType == 0 && eLevel > 11) ? 1 : 0);
        public int maxRolls => gearType == 0 ? 4 : 5;
        public List<Substat> subs = new List<Substat>();


        public int[] idealRolls = new[] { 0, 0, 0, 0 };
        public int idealIncrease = 0;

        public void CalculateIdealRolls() {
            while (idealRolls.Sum() + rolls < maxRolls) {
                var maxIncrease = 0;
                var index = 0;
                for (int i = 0; i < subs.Count; i++) {
                    var sub = subs[i];
                    int increase = (int)((sub.reforgeValues[sub.rolls + idealRolls[i]] - sub.reforgeValues[sub.rolls + idealRolls[i] - 1] + sub.maxRoll[gearType]) * sub.scoreMulti);
                    if (increase > maxIncrease) {
                        maxIncrease = increase;
                        index = i;
                    }
                }
                idealRolls[index]++;
                idealIncrease += maxIncrease;
            }
            idealIncrease += (gearType == 0 && eLevel < 12) ? 8 : 0; // If its a heroic piece below 12, just add 8. It's prety likely this is the best outcome of a new sub.
        }

        public void AttemptToAssignRollCounts() {
            var rollsToDistribute = rolls;
            foreach (var s in subs) {
                s.rolls = s.minPotentialRolls(gearType);
                rollsToDistribute -= s.rolls - 1; // This removes the 1 initial roll gear alwways has
            }
            while (rollsToDistribute > 0) {
                var likelyTarget = subs.OrderByDescending(x => x.maxPotentialRolls(gearType) - x.rolls).First();
                likelyTarget.rolls++;
                rollsToDistribute--;
            }
        }
        public void SetGearEnhanceFromString(string s) {
            int level = 0;
            int.TryParse(s.Trim('+', '\n'), out level);
            eLevel = level;
        }
        public void SetGearTypeFromString(string s) {
            var toks = s.Split(' ');
            switch (toks[0]) {
                case "Heroic":
                    gearType = 0;
                    break;
                default:
                    gearType = 1;
                    break;
            }
        }
        // This shit should probaly be in another file, but fuck it
        // Levenshtein Dist calc from https://www.dotnetperls.com/levenshtein
        public int LevenshteinDist(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
                return m;
            if (m == 0)
                return n;
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            // Begin looping.
            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    // Compute cost.
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
                }
            }
            // Return cost.
            return d[n, m];
        }

        private readonly string[] substatNames = { "Attack", "Defense", "Health", "Effectiveness", "Effect Resistance", "Critical Hit Damage", "Critical Hit Chance", "Speed" };
        public void AddSubstatsFromString(string[] toks) {
            for (var i = 0; i < toks.Count(); i++) {
                var innerToks = toks[i].Split(' ');
                int value = 0;
                int index = 0;
                while (!int.TryParse(innerToks[index].Trim('+', '%'), out value)) {
                    index++;
                    if (index > innerToks.Length)
                        break;
                }
                if (index > 0) {
                    var flatValue = !innerToks[index].Contains('%');
                    var targetSubName = string.Join(" ", innerToks.Take(index));
                    var minDist = int.MaxValue;
                    var subName = string.Empty;
                    foreach (var substatName in substatNames) {
                        var dist = LevenshteinDist(targetSubName, substatName);
                        if (dist < minDist) {
                            minDist = dist;
                            subName = substatName;
                        }
                    }

                    switch (subName) {
                        case "Attack":
                            if (flatValue)
                                subs.Add(new Sub_FlatAttack(value));
                            else
                                subs.Add(new AttackPercent(value));
                            break;
                        case "Defense":
                            if (flatValue)
                                subs.Add(new Sub_FlatDefense(value));
                            else
                                subs.Add(new Sub_DefensePercent(value));
                            break;
                        case "Health":
                            if (flatValue)
                                subs.Add(new Sub_FlatHealth(value));
                            else
                                subs.Add(new Sub_HealthPercent(value));
                            break;
                        case "Effectiveness":
                            subs.Add(new Sub_Effectiveness(value));
                            break;
                        case "Effect Resistance":
                            subs.Add(new Sub_EffectResistance(value));
                            break;
                        case "Speed":
                            subs.Add(new Sub_Speed(value));
                            break;
                        case "Critical Hit Damage":
                            subs.Add(new Sub_CritDamage(value));
                            break;
                        case "Critical Hit Chance":
                            subs.Add(new Sub_CritChance(value));
                            break;
                        default:
                            break;
                    }
                } else {
                    Debug.WriteLine("Error: Failed to parse a substat value");
                }
            }
        }
        public override string ToString() {
            var sb = new StringBuilder();
            foreach (var sub in subs)
                sb.AppendLine(sub.ToString());
            sb.AppendLine($"GEARSCORE: {subs.Sum(x => x.gearScoreValue)}");
            return sb.ToString();
        }

        public float gearscore => subs.Sum(x => x.gearScoreValue);
        public float gearscoreReforge => subs.Sum(x => x.gearScoreValReforge);
    }
}
