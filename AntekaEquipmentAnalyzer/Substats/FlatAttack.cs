namespace AntekaEquipmentAnalyzer {
    public class FlatAttack : Substat {
        public override string name => "Attack";
        public FlatAttack(int val) : base(val) { }
        public override float scoreMulti => 3.46f / 39f;
        public override int[] maxRoll => new[] { 44, 46 };
        public override int[] minRoll => new[] { 31, 33 };
        public override int[] reforgeValues => new[] { 11, 22, 33, 44, 55, 66 };
    }
}
