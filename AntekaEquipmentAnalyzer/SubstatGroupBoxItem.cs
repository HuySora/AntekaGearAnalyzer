using System.Drawing;
using System.Windows.Forms;

namespace AntekaEquipmentAnalyzer {
    public class SubstatGroupBoxItem {
        public TextBox textBox_Value { get; private set; }
        public ProgressBar progressBar_Percent { get; private set; }
        public GroupBox groupBox_Substat { get; private set; }
        public ComboBox modifyComboBox { get; private set; }
        public TextBox textBox_ReforgeValue { get; private set; }
        public Label label_Arrow { get; private set; }
        public Label label_Percent { get; private set; }
        public Label label_ValueMax { get; private set; }
        public Label label_Rolls { get; private set; }
        public Label label_RollsLabel { get; private set; }
        public SubstatGroupBoxItem(Substat s, int type) {
            // Create
            textBox_Value = new TextBox();
            progressBar_Percent = new ProgressBar();
            groupBox_Substat = new GroupBox();
            modifyComboBox = new ComboBox();
            textBox_ReforgeValue = new TextBox();
            label_Arrow = new Label();
            label_Percent = new Label();
            label_ValueMax = new Label();
            label_Rolls = new Label();
            label_RollsLabel = new Label();

            // 
            // textBox_Value
            // 
            textBox_Value.AcceptsReturn = true;
            textBox_Value.Location = new Point(47, 28);
            textBox_Value.Name = "textBox_Value";
            textBox_Value.ReadOnly = true;
            textBox_Value.Size = new Size(67, 21);
            textBox_Value.TabIndex = 4;
            textBox_Value.Text = $"{s.Value}";
            // 
            // progressBar_Percent
            // 
            progressBar_Percent.BackColor = SystemColors.Control;
            progressBar_Percent.ForeColor = Color.Lime;
            progressBar_Percent.Location = new Point(47, 60);
            progressBar_Percent.Name = "progressBar_Percent";
            progressBar_Percent.Size = new Size(186, 10);
            progressBar_Percent.TabIndex = 5;
            progressBar_Percent.Value = (int)s.percentVal(type);
            // 
            // groupBox_Substat
            // 
            groupBox_Substat.Controls.Add(modifyComboBox);
            groupBox_Substat.Controls.Add(textBox_ReforgeValue);
            groupBox_Substat.Controls.Add(label_Arrow);
            groupBox_Substat.Controls.Add(label_Percent);
            groupBox_Substat.Controls.Add(label_ValueMax);
            groupBox_Substat.Controls.Add(label_Rolls);
            groupBox_Substat.Controls.Add(label_RollsLabel);
            groupBox_Substat.Controls.Add(textBox_Value);
            groupBox_Substat.Controls.Add(progressBar_Percent);
            groupBox_Substat.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            groupBox_Substat.Location = new Point(3, 3);
            groupBox_Substat.Name = "groupBox_Substat";
            groupBox_Substat.Size = new Size(286, 82);
            groupBox_Substat.TabIndex = 6;
            groupBox_Substat.TabStop = false;
            groupBox_Substat.Text = s.Name;
            // 
            // modifyComboBox
            // 
            modifyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modifyComboBox.FormattingEnabled = true;
            modifyComboBox.Items.AddRange(new object[] {
            "Attack %",
            "Defense %",
            "Health %",
            "Effectiveness %",
            "Effect Resistance %",
            "Speed",
            "Critical Hit Damage %",
            "Critical Hit Chance %",
            "Attack",
            "Defense",
            "Health"});
            modifyComboBox.Location = new Point(142, -3);
            modifyComboBox.Name = "modifyComboBox";
            modifyComboBox.Size = new Size(144, 23);
            modifyComboBox.TabIndex = 12;
            // 
            // textBox_ReforgeValue
            // 
            textBox_ReforgeValue.AcceptsReturn = true;
            textBox_ReforgeValue.Location = new Point(195, 28);
            textBox_ReforgeValue.Name = "textBox_ReforgeValue";
            textBox_ReforgeValue.ReadOnly = true;
            textBox_ReforgeValue.Size = new Size(67, 21);
            textBox_ReforgeValue.TabIndex = 11;
            textBox_ReforgeValue.Text = $"{s.ReforgedValue}";
            // 
            // label_Arrow
            // 
            label_Arrow.AutoSize = true;
            label_Arrow.Location = new Point(162, 31);
            label_Arrow.Name = "label_Arrow";
            label_Arrow.Size = new Size(21, 15);
            label_Arrow.TabIndex = 10;
            label_Arrow.Text = ">>";
            // 
            // label_Percent
            // 
            label_Percent.AutoSize = true;
            label_Percent.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_Percent.Location = new Point(239, 57);
            label_Percent.Name = "label_Percent";
            label_Percent.Size = new Size(28, 13);
            label_Percent.TabIndex = 9;
            label_Percent.Text = $"{(int)s.percentVal(type)}%";
            // 
            // label_ValueMax
            // 
            label_ValueMax.AutoSize = true;
            label_ValueMax.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_ValueMax.Location = new Point(120, 41);
            label_ValueMax.Name = "label_ValueMax";
            label_ValueMax.Size = new Size(25, 13);
            label_ValueMax.TabIndex = 8;
            label_ValueMax.Text = $"/ {s.maxPossibleValue(type)}";
            // 
            // label_Rolls
            // 
            label_Rolls.AutoSize = true;
            label_Rolls.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_Rolls.Location = new Point(8, 21);
            label_Rolls.Name = "label_Rolls";
            label_Rolls.Size = new Size(33, 37);
            label_Rolls.TabIndex = 7;
            label_Rolls.Text = $"{s.rolls - 1}";
            // 
            // label_RollsLabel
            // 
            label_RollsLabel.AutoSize = true;
            label_RollsLabel.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_RollsLabel.Location = new Point(10, 57);
            label_RollsLabel.Name = "label_RollsLabel";
            label_RollsLabel.Size = new Size(29, 13);
            label_RollsLabel.TabIndex = 6;
            label_RollsLabel.Text = "Rolls";
        }
        public GroupBox GetRootGroupBox() => groupBox_Substat;
    }
}
