namespace AntekaEquipmentAnalyzer {
    public class CritDamage : Substat {
        public override string Name => "Crit Damage";
        public CritDamage(int val) : base(val) { }
        public override float ScoreMultiplier => 8f / 7f;
        public override int[] MaxRolls => new[] { 7, 7 };
        public override int[] reforgeValues => new[] { 1, 2, 3, 4, 6, 7 };
    }
}
