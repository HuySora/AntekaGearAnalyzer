using System;

namespace AntekaEquipmentAnalyzer {
    /* Fribbles GearScore Calc:
     * Score = Attack %
     * + Defense %
     * + Hp %
     * + Effectiveness
     * + Effect Resistance
     * + Speed * (8/4)
     * + Crit Damage * (8/7)
     * + Crit Chance * (8/5)
     * + Flat Attack * 3.46 / 39
     * + Flat Defense * 4.99 / 31
     * + Flat Hp * 3.09 / 174
     */
    public abstract class Substat {
        public int value;
        public int valueReforged => value + reforgeValues[rolls - 1];
        public int rolls = 1; // How many rolls have gone into this stat - its going to be a guess.
        public virtual string name => "Substat";
        public virtual float scoreMulti => 1;
        public virtual int[] maxRoll => new[] { 8, 8 };
        public virtual int[] minRoll => new[] { 4, 4 };
        public virtual int[] reforgeValues => new[] { 1, 3, 4, 5, 7, 8 };

        public Substat(int val) {
            value = val;
        }

        public float maxPotentialRolls(int type) => (float)value / minRoll[type];
        public int minPotentialRolls(int type) => (int)Math.Ceiling(((double)value / maxRoll[type]));
        public int maxPossibleValue(int type) => rolls * maxRoll[type];
        public int minPossibleValue(int type) => rolls * minRoll[type];
        public float gearScoreValue => value * scoreMulti;
        public float maxPossibleGearScoreValue(int type) => maxPossibleValue(type) * scoreMulti;
        public float minPossibleGearScoreValue(int type) => minPossibleValue(type) * scoreMulti;
        public float gearScoreValReforge => valueReforged * scoreMulti;
        public float percentVal(int type) => (value - minPossibleValue(type)) / (float)(maxPossibleValue(type) - minPossibleValue(type)) * 100f;
        public override string ToString() => $"{name} : {value}";
    }
}
