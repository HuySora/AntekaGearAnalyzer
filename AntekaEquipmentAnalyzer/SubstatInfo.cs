using System.Drawing;
using System.Windows.Forms;

namespace AntekaEquipmentAnalyzer {
    public class SubstatInfo {
        public TextBox textBox_Value, textBox_ReforgeValue;
        public ProgressBar progressBar_Percent;
        public GroupBox groupBox_Substat;
        public Label label_Percent, label_ValueMax, label_Rolls, label_RollsLabel, label_Arrow;
        public SubstatInfo(Substat s, int type) {
            //Group Box
            groupBox_Substat = new GroupBox();
            groupBox_Substat.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            groupBox_Substat.Location = new Point(3, 3);
            groupBox_Substat.Name = "groupBox_Substat";
            groupBox_Substat.Size = new Size(286, 82);
            groupBox_Substat.Text = s.name;

            //Progress Bar
            progressBar_Percent = new ProgressBar();
            progressBar_Percent.BackColor = SystemColors.Control;
            progressBar_Percent.ForeColor = Color.Lime;
            progressBar_Percent.Location = new Point(47, 60);
            progressBar_Percent.Size = new Size(186, 10);
            progressBar_Percent.Value = (int)s.percentVal(type);

            //Value Textbox
            textBox_Value = new TextBox();
            textBox_Value.Location = new Point(47, 28);
            textBox_Value.ReadOnly = true;
            textBox_Value.Size = new Size(67, 26);
            textBox_Value.Text = $"{s.Value}";

            //Roll Label
            label_RollsLabel = new Label();
            label_RollsLabel.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_RollsLabel.Location = new Point(10, 57);
            label_RollsLabel.Size = new Size(29, 13);
            label_RollsLabel.Text = "Rolls";

            //Roll Value Label
            label_Rolls = new Label();
            label_Rolls.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_Rolls.Location = new Point(8, 21);
            label_Rolls.Size = new Size(33, 37);
            label_Rolls.Text = $"{s.rolls - 1}";

            //Max Value Label
            label_ValueMax = new Label();
            label_ValueMax.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_ValueMax.Location = new Point(120, 41);
            label_ValueMax.Name = "label_ValueMax";
            label_ValueMax.Size = new Size(35, 13);
            label_ValueMax.Text = $"/ {s.maxPossibleValue(type)}";

            //Percent Label
            label_Percent = new Label();
            label_Percent.Font = new Font("Microsoft Sans Serif", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            label_Percent.Location = new Point(239, 57);
            label_Percent.Size = new Size(40, 13);
            label_Percent.Text = $"{(int)s.percentVal(type)}%";

            //Arrow Label
            label_Arrow = new Label();
            label_Arrow.Location = new Point(162, 31);
            label_Arrow.Size = new Size(27, 20);
            label_Arrow.Text = ">>";

            //Reforge Value Textbox
            textBox_ReforgeValue = new TextBox();
            textBox_ReforgeValue.Location = new Point(195, 28);
            textBox_ReforgeValue.ReadOnly = true;
            textBox_ReforgeValue.Size = new Size(67, 26);
            textBox_ReforgeValue.Text = $"{s.ReforgedValue}";

            groupBox_Substat.Controls.Add(label_Percent);
            groupBox_Substat.Controls.Add(label_ValueMax);
            groupBox_Substat.Controls.Add(label_Rolls);
            groupBox_Substat.Controls.Add(label_RollsLabel);
            groupBox_Substat.Controls.Add(label_Arrow);
            groupBox_Substat.Controls.Add(textBox_Value);
            groupBox_Substat.Controls.Add(textBox_ReforgeValue);
            groupBox_Substat.Controls.Add(progressBar_Percent);
        }
    }
}
