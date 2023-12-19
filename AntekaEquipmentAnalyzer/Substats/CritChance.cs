namespace AntekaEquipmentAnalyzer {
    public class CritChance : Substat {
        public override string Name => "Crit Chance";
        public CritChance(int val) : base(val) { }
        public override float ScoreMultiplier => 8f / 5f;
        public override int[] MaxRolls => new[] { 5, 5 };
        public override int[] MinRolls => new[] { 3, 3 };
        public override int[] ReforgeValues => new[] { 1, 2, 3, 4, 5, 6 };
    }
}
