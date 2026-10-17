namespace ArcadeMaker.IDE.Editors.Object.ObjectProperties
{
    partial class ObjectPropertiesModifier
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox = new GroupBox();
            grid = new DataGridView();
            NameColumn = new DataGridViewTextBoxColumn();
            TypeColumn = new DataGridViewComboBoxColumn();
            ValueColumn = new DataGridViewTextBoxColumn();
            ModifiersColumn = new DataGridViewTextBoxColumn();
            buttonsPanel = new TableLayoutPanel();
            AddBtn = new Button();
            DeleteBtn = new Button();
            UpBtn = new Button();
            DownBtn = new Button();
            groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            buttonsPanel.SuspendLayout();
            SuspendLayout();
            //
            // groupBox
            //
            groupBox.Controls.Add(grid);
            groupBox.Dock = DockStyle.Fill;
            groupBox.Location = new Point(0, 0);
            groupBox.Name = "groupBox";
            groupBox.Padding = new Padding(6, 4, 6, 6);
            groupBox.Size = new Size(325, 318);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Text = "Properties";
            //
            // grid
            //
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = SystemColors.Window;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Columns.AddRange(new DataGridViewColumn[] { NameColumn, TypeColumn, ValueColumn, ModifiersColumn });
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(6, 20);
            grid.Name = "grid";
            grid.RowHeadersWidth = 20;
            grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            grid.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            grid.Size = new Size(313, 292);
            grid.TabIndex = 0;
            grid.CellClick += grid_CellClick;
            grid.CellPainting += grid_CellPainting;
            grid.CellValueChanged += grid_CellValueChanged;
            grid.CurrentCellChanged += grid_CurrentCellChanged;
            grid.CurrentCellDirtyStateChanged += grid_CurrentCellDirtyStateChanged;
            grid.DataError += grid_DataError;
            grid.KeyDown += grid_KeyDown;
            //
            // NameColumn
            //
            NameColumn.FillWeight = 35F;
            NameColumn.HeaderText = "Name";
            NameColumn.Name = "NameColumn";
            NameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            NameColumn.ToolTipText = "Property name";
            //
            // TypeColumn
            //
            TypeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            TypeColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            TypeColumn.DisplayStyleForCurrentCellOnly = true;
            TypeColumn.FlatStyle = FlatStyle.Flat;
            TypeColumn.HeaderText = "Type";
            TypeColumn.Name = "TypeColumn";
            TypeColumn.ToolTipText = "Property type";
            TypeColumn.Width = 70;
            //
            // ValueColumn
            //
            ValueColumn.FillWeight = 45F;
            ValueColumn.HeaderText = "Value";
            ValueColumn.Name = "ValueColumn";
            ValueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            ValueColumn.ToolTipText = "Initial value (an Exp expression; may reference properties above it)";
            // 
            // ModifiersColumn
            // 
            ModifiersColumn.FillWeight = 40F;
            ModifiersColumn.HeaderText = "Modifiers";
            ModifiersColumn.Name = "ModifiersColumn";
            ModifiersColumn.ReadOnly = true;
            ModifiersColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            ModifiersColumn.ToolTipText = "Constant, Private, Nullable";
            //
            // buttonsPanel
            //
            buttonsPanel.ColumnCount = 4;
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
            buttonsPanel.Controls.Add(AddBtn, 0, 0);
            buttonsPanel.Controls.Add(DeleteBtn, 1, 0);
            buttonsPanel.Controls.Add(UpBtn, 2, 0);
            buttonsPanel.Controls.Add(DownBtn, 3, 0);
            buttonsPanel.Dock = DockStyle.Bottom;
            buttonsPanel.Location = new Point(0, 318);
            buttonsPanel.Name = "buttonsPanel";
            buttonsPanel.RowCount = 1;
            buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            buttonsPanel.Size = new Size(325, 29);
            buttonsPanel.TabIndex = 1;
            //
            // AddBtn
            //
            AddBtn.Dock = DockStyle.Fill;
            AddBtn.Name = "AddBtn";
            AddBtn.TabIndex = 0;
            AddBtn.Text = "Add";
            AddBtn.UseVisualStyleBackColor = true;
            AddBtn.Click += AddBtn_Click;
            //
            // DeleteBtn
            //
            DeleteBtn.Dock = DockStyle.Fill;
            DeleteBtn.Enabled = false;
            DeleteBtn.Name = "DeleteBtn";
            DeleteBtn.TabIndex = 1;
            DeleteBtn.Text = "Delete";
            DeleteBtn.UseVisualStyleBackColor = true;
            DeleteBtn.Click += DeleteBtn_Click;
            //
            // UpBtn
            //
            UpBtn.Dock = DockStyle.Fill;
            UpBtn.Enabled = false;
            UpBtn.Name = "UpBtn";
            UpBtn.TabIndex = 2;
            UpBtn.Text = "▲";
            UpBtn.UseVisualStyleBackColor = true;
            UpBtn.Click += UpBtn_Click;
            //
            // DownBtn
            //
            DownBtn.Dock = DockStyle.Fill;
            DownBtn.Enabled = false;
            DownBtn.Name = "DownBtn";
            DownBtn.TabIndex = 3;
            DownBtn.Text = "▼";
            DownBtn.UseVisualStyleBackColor = true;
            DownBtn.Click += DownBtn_Click;
            //
            // ObjectPropertiesModifier
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox);
            Controls.Add(buttonsPanel);
            Name = "ObjectPropertiesModifier";
            Size = new Size(325, 347);
            groupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            buttonsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox;
        private DataGridView grid;
        private DataGridViewTextBoxColumn NameColumn;
        private DataGridViewComboBoxColumn TypeColumn;
        private DataGridViewTextBoxColumn ValueColumn;
        private DataGridViewTextBoxColumn ModifiersColumn;
        private TableLayoutPanel buttonsPanel;
        private Button AddBtn;
        private Button DeleteBtn;
        private Button UpBtn;
        private Button DownBtn;
    }
}
