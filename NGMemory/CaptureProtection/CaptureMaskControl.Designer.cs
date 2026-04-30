namespace NGMemory.CaptureProtection
{
    partial class CaptureMaskControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.CheckBox protectCheckBox;
        private System.Windows.Forms.CheckBox borderOnlyCheckBox;
        private System.Windows.Forms.CheckBox topMostCheckBox;
        private System.Windows.Forms.CheckBox allowResizeCheckBox;
        private System.Windows.Forms.CheckBox allowInteractionCheckBox;
        private System.Windows.Forms.CheckBox bindToOwnerCheckBox;
        private System.Windows.Forms.ComboBox effectComboBox;
        private System.Windows.Forms.NumericUpDown xInput;
        private System.Windows.Forms.NumericUpDown yInput;
        private System.Windows.Forms.NumericUpDown widthInput;
        private System.Windows.Forms.NumericUpDown heightInput;
        private System.Windows.Forms.NumericUpDown borderInput;
        private System.Windows.Forms.TrackBar opacitySlider;
        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.Label debugLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (manager != null)
                {
                    manager.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.createButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.protectCheckBox = new System.Windows.Forms.CheckBox();
            this.borderOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.topMostCheckBox = new System.Windows.Forms.CheckBox();
            this.allowResizeCheckBox = new System.Windows.Forms.CheckBox();
            this.allowInteractionCheckBox = new System.Windows.Forms.CheckBox();
            this.bindToOwnerCheckBox = new System.Windows.Forms.CheckBox();
            this.effectComboBox = new System.Windows.Forms.ComboBox();
            this.xInput = new System.Windows.Forms.NumericUpDown();
            this.yInput = new System.Windows.Forms.NumericUpDown();
            this.widthInput = new System.Windows.Forms.NumericUpDown();
            this.heightInput = new System.Windows.Forms.NumericUpDown();
            this.borderInput = new System.Windows.Forms.NumericUpDown();
            this.opacitySlider = new System.Windows.Forms.TrackBar();
            this.hintLabel = new System.Windows.Forms.Label();
            this.debugLabel = new System.Windows.Forms.Label();
            this.rootTable.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.borderInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacitySlider)).BeginInit();
            this.SuspendLayout();
            // 
            // rootTable
            // 
            this.rootTable.AutoSize = true;
            this.rootTable.ColumnCount = 2;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.titleLabel, 0, 0);
            this.rootTable.Controls.Add(this.buttonPanel, 0, 1);
            this.rootTable.Controls.Add(this.protectCheckBox, 0, 2);
            this.rootTable.Controls.Add(this.borderOnlyCheckBox, 0, 3);
            this.rootTable.Controls.Add(this.topMostCheckBox, 0, 4);
            this.rootTable.Controls.Add(this.allowResizeCheckBox, 0, 5);
            this.rootTable.Controls.Add(this.allowInteractionCheckBox, 0, 6);
            this.rootTable.Controls.Add(this.bindToOwnerCheckBox, 0, 7);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Effekt", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 8);
            this.rootTable.Controls.Add(this.effectComboBox, 1, 8);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "X", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 9);
            this.rootTable.Controls.Add(this.xInput, 1, 9);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Y", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 10);
            this.rootTable.Controls.Add(this.yInput, 1, 10);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Breite", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 11);
            this.rootTable.Controls.Add(this.widthInput, 1, 11);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Hoehe", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 12);
            this.rootTable.Controls.Add(this.heightInput, 1, 12);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Border", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 13);
            this.rootTable.Controls.Add(this.borderInput, 1, 13);
            this.rootTable.Controls.Add(new System.Windows.Forms.Label { Text = "Opacity", TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = System.Windows.Forms.DockStyle.Fill }, 0, 14);
            this.rootTable.Controls.Add(this.opacitySlider, 1, 14);
            this.rootTable.Controls.Add(this.hintLabel, 0, 15);
            this.rootTable.Controls.Add(this.debugLabel, 0, 16);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.RowCount = 17;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.rootTable.SetColumnSpan(this.titleLabel, 2);
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Text = "Freie Capture-Maske";
            // 
            // buttonPanel
            // 
            this.buttonPanel.AutoSize = true;
            this.rootTable.SetColumnSpan(this.buttonPanel, 2);
            this.buttonPanel.Controls.Add(this.createButton);
            this.buttonPanel.Controls.Add(this.removeButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // createButton
            // 
            this.createButton.Size = new System.Drawing.Size(140, 28);
            this.createButton.Text = "Maske erstellen";
            // 
            // removeButton
            // 
            this.removeButton.Size = new System.Drawing.Size(140, 28);
            this.removeButton.Text = "Maske entfernen";
            // 
            // checkboxes
            // 
            this.rootTable.SetColumnSpan(this.protectCheckBox, 2);
            this.rootTable.SetColumnSpan(this.borderOnlyCheckBox, 2);
            this.rootTable.SetColumnSpan(this.topMostCheckBox, 2);
            this.rootTable.SetColumnSpan(this.allowResizeCheckBox, 2);
            this.rootTable.SetColumnSpan(this.allowInteractionCheckBox, 2);
            this.rootTable.SetColumnSpan(this.bindToOwnerCheckBox, 2);
            this.protectCheckBox.Text = "Maske vor Capture schuetzen";
            this.borderOnlyCheckBox.Text = "Nur Border fuer Nutzer anzeigen";
            this.topMostCheckBox.Text = "Immer im Vordergrund";
            this.allowResizeCheckBox.Text = "Per Maus resizen";
            this.allowInteractionCheckBox.Text = "Mausinteraktion aktiv";
            this.bindToOwnerCheckBox.Text = "An Besitzerfenster binden";
            this.protectCheckBox.AutoSize = true;
            this.borderOnlyCheckBox.AutoSize = true;
            this.topMostCheckBox.AutoSize = true;
            this.allowResizeCheckBox.AutoSize = true;
            this.allowInteractionCheckBox.AutoSize = true;
            this.bindToOwnerCheckBox.AutoSize = true;
            // 
            // inputs
            // 
            this.effectComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.effectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.effectComboBox.Items.AddRange(new object[] { "Schwarz", "Placeholder-Text", "Simulierter Blur, nur Demo" });
            this.xInput.Minimum = -20000;
            this.xInput.Maximum = 20000;
            this.yInput.Minimum = -20000;
            this.yInput.Maximum = 20000;
            this.widthInput.Minimum = 80;
            this.widthInput.Maximum = 8000;
            this.heightInput.Minimum = 60;
            this.heightInput.Maximum = 8000;
            this.borderInput.Minimum = 1;
            this.borderInput.Maximum = 30;
            this.opacitySlider.Minimum = 0;
            this.opacitySlider.Maximum = 100;
            this.opacitySlider.TickFrequency = 10;
            this.xInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.widthInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heightInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.borderInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.opacitySlider.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // hintLabel
            // 
            this.hintLabel.AutoSize = false;
            this.rootTable.SetColumnSpan(this.hintLabel, 2);
            this.hintLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hintLabel.Height = 74;
            this.hintLabel.Text = "Schwarz nutzt eine gefuellte Flaeche + WDA_MONITOR. Opacity 0 nutzt intern 1%, damit Capture eine echte Fensterflaeche sieht. Interaktion kann deaktiviert werden.";
            // 
            // debugLabel
            // 
            this.debugLabel.AutoSize = false;
            this.rootTable.SetColumnSpan(this.debugLabel, 2);
            this.debugLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugLabel.Height = 82;
            this.debugLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // CaptureMaskControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rootTable);
            this.Name = "CaptureMaskControl";
            this.Size = new System.Drawing.Size(360, 560);
            this.rootTable.ResumeLayout(false);
            this.rootTable.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.widthInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heightInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.borderInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacitySlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
