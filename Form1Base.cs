namespace WindowsFormsApp1
{
    public class Form1Base : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing); // Call the base class Dispose method
        }
    }
}