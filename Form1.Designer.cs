using System.Windows.Forms;

namespace LabWork
{
    partial class Form1
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "Form1";
            // Залишаємо підписку лише тут, прибираючи її з конструктора Form1.cs
            this.Paint += new PaintEventHandler(this.Form1_Paint); 
            this.ResumeLayout(false);
        }
    }
}