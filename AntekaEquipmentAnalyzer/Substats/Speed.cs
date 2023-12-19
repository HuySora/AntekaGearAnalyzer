namespace AntekaEquipmentAnalyzer {
    public class Speed : Substat {
        public override string Name => "Speed";
        public Speed(int val) : base(val) { }
        public override float ScoreMultiplier => 2;
        public override int[] MaxRolls => new[] { 4, 4 }; // Fuck 5 speed rolls
        public override int[] MinRolls => new[] { 1, 1 };
        public override int[] ReforgeValues => new[] { 0, 1, 2, 3, 4, 4 };
    }
}
