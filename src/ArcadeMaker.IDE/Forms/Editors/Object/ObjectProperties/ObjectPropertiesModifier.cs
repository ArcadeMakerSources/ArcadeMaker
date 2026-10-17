using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE.Editors.Object.ObjectProperties;

public partial class ObjectPropertiesModifier : UserControl
{
    public GameObject GameObject { get; private set; }

    private static readonly Color errorColor = Color.MistyRose;
    private readonly System.Windows.Forms.Timer debugTimer = new() { Interval = 3000 };
    private bool isLoadingRow = false;

    private readonly ContextMenuStrip modifiersMenu = new();
    private readonly ToolStripMenuItem constantItem = new("Constant") { CheckOnClick = true };
    private readonly ToolStripMenuItem privateItem = new("Private") { CheckOnClick = true };
    private readonly ToolStripMenuItem nullableItem = new("Nullable") { CheckOnClick = true, ToolTipText = "A property of type Any is always nullable" };
    private DataGridViewRow? modifiersMenuRow;

    public ObjectPropertiesModifier(GameObject? gameObject = null) : this()
    {
        if (gameObject != null)
        {
            Init(gameObject);
        }
    }

    public ObjectPropertiesModifier()
    {
        InitializeComponent();

        foreach (var type in Enum.GetValues<VariableType>())
            TypeColumn.Items.Add(type);
        TypeColumn.ValueType = typeof(VariableType);

        debugTimer.Tick += async (s, e) =>
        {
            // check for errors
            debugTimer.Stop();
            await Task.Run(() => Debugging.Debug.TryBuild());
        };
        // leave room for the drop down arrow painted in grid_CellPainting
        ModifiersColumn.DefaultCellStyle.Padding = new Padding(0, 0, SystemInformation.VerticalScrollBarWidth, 0);

        modifiersMenu.Items.AddRange(constantItem, privateItem, nullableItem);
        modifiersMenu.ShowItemToolTips = true;
        foreach (ToolStripMenuItem item in modifiersMenu.Items)
            item.CheckedChanged += ModifierItem_CheckedChanged;
        // keep the menu open while toggling modifiers, like a multi-select combo box
        modifiersMenu.Closing += (s, e) => e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        modifiersMenu.Closed += (s, e) => modifiersMenuRow = null;

        Disposed += (s, e) =>
        {
            debugTimer.Dispose();
            modifiersMenu.Dispose();
        };
    }

    public void Init(GameObject gameObject)
    {
        this.GameObject = gameObject;

        grid.Rows.Clear();
        foreach (var pro in GameObject.ExtraProperties)
        {
            AddRow(pro);
        }
        UpdateButtons();
    }

    private static IDEObjectProperty PropertyOf(DataGridViewRow row) => (IDEObjectProperty)row.Tag!;

    private DataGridViewRow AddRow(IDEObjectProperty pro)
    {
        isLoadingRow = true;
        int index = grid.Rows.Add(pro.Name, pro.Type, pro.InitValueCode, ModifiersText(pro));
        DataGridViewRow row = grid.Rows[index];
        row.Tag = pro;
        isLoadingRow = false;
        return row;
    }

    private static string ModifiersText(IDEObjectProperty pro)
    {
        List<string> modifiers = [];
        if (pro.Constant)
            modifiers.Add("Constant");
        if (pro.Private)
            modifiers.Add("Private");
        if (pro.Nullable)
            modifiers.Add("Nullable");
        return string.Join(", ", modifiers);
    }

    private void UpdateModifiersCell(DataGridViewRow row)
    {
        isLoadingRow = true;
        row.Cells[ModifiersColumn.Index].Value = ModifiersText(PropertyOf(row));
        isLoadingRow = false;
    }

    private void ShowModifiersMenu(DataGridViewRow row)
    {
        IDEObjectProperty pro = PropertyOf(row);

        modifiersMenuRow = null; // don't treat the initial check states as user changes
        constantItem.Checked = pro.Constant;
        privateItem.Checked = pro.Private;
        nullableItem.Checked = pro.Nullable;
        nullableItem.Enabled = pro.Type != VariableType.Any;
        modifiersMenuRow = row;

        Rectangle rect = grid.GetCellDisplayRectangle(ModifiersColumn.Index, row.Index, false);
        modifiersMenu.MinimumSize = new(rect.Width, 0);
        modifiersMenu.Show(grid, new Point(rect.Left, rect.Bottom));
    }

    private void ModifierItem_CheckedChanged(object? sender, EventArgs e)
    {
        if (modifiersMenuRow == null)
            return;

        IDEObjectProperty pro = PropertyOf(modifiersMenuRow);
        pro.Constant = constantItem.Checked;
        pro.Private = privateItem.Checked;
        pro.Nullable = nullableItem.Checked;
        UpdateModifiersCell(modifiersMenuRow);

        PropertiesChanged();
    }

    private static void SetCellError(DataGridViewCell cell, string? error)
    {
        cell.ErrorText = error ?? "";
        cell.Style.BackColor = error == null ? Color.Empty : errorColor;
    }

    /// <summary>
    /// Re-checks every name cell, since renaming or deleting one property can make another one's name valid (or invalid) again.
    /// </summary>
    private void RevalidateNames()
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            IDEObjectProperty pro = PropertyOf(row);
            DataGridViewCell cell = row.Cells[NameColumn.Index];
            string name = cell.Value as string ?? "";

            if (name == pro.Name || pro.IsValidName(name))
            {
                pro.Name = name;
                SetCellError(cell, null);
            }
            else
                SetCellError(cell, "Invalid or duplicate property name");
        }
    }

    private void PropertiesChanged()
    {
        debugTimer.Stop();
        debugTimer.Start();
    }

    private void UpdateButtons()
    {
        int index = grid.CurrentCell?.RowIndex ?? -1;
        DeleteBtn.Enabled = index >= 0;
        UpBtn.Enabled = index > 0;
        DownBtn.Enabled = index >= 0 && index < grid.Rows.Count - 1;
    }

    private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (isLoadingRow || e.RowIndex < 0)
            return;

        DataGridViewRow row = grid.Rows[e.RowIndex];
        DataGridViewCell cell = row.Cells[e.ColumnIndex];
        IDEObjectProperty pro = PropertyOf(row);

        if (e.ColumnIndex == NameColumn.Index)
        {
            RevalidateNames();
        }
        else if (e.ColumnIndex == TypeColumn.Index)
        {
            VariableType selection = cell.Value as VariableType? ?? VariableType.Any;
            pro.Type = selection;

            // a property of type Any is always nullable
            if (selection == VariableType.Any)
            {
                pro.Nullable = true;
                UpdateModifiersCell(row);
            }

            row.Cells[ValueColumn.Index].Value = IDEObjectProperty.GetDefaultInitVal(selection);
        }
        else if (e.ColumnIndex == ValueColumn.Index)
        {
            string code = cell.Value as string ?? "";
            if (string.IsNullOrWhiteSpace(code))
                SetCellError(cell, "The initial value can't be empty");
            else
            {
                pro.InitValueCode = code;
                SetCellError(cell, null);
            }
        }

        PropertiesChanged();
    }

    private void grid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
        // commit type changes immediately instead of when leaving the cell
        if (grid.CurrentCell is DataGridViewComboBoxCell && grid.IsCurrentCellDirty)
            grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }

    private void grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != ModifiersColumn.Index || e.Graphics == null)
            return;

        // draw the modifiers cell like a combo box
        e.Paint(e.CellBounds, DataGridViewPaintParts.All);

        int buttonWidth = SystemInformation.VerticalScrollBarWidth;
        Rectangle button = new(e.CellBounds.Right - buttonWidth - 1, e.CellBounds.Top + 1, buttonWidth, e.CellBounds.Height - 3);
        if (ComboBoxRenderer.IsSupported)
            ComboBoxRenderer.DrawDropDownButton(e.Graphics, button, System.Windows.Forms.VisualStyles.ComboBoxState.Normal);
        else
            ControlPaint.DrawComboButton(e.Graphics, button, ButtonState.Flat);

        e.Handled = true;
    }

    private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        if (e.ColumnIndex == ModifiersColumn.Index)
        {
            ShowModifiersMenu(grid.Rows[e.RowIndex]);
            return;
        }

        // start editing on a single click, and open the type list right away
        grid.BeginEdit(true);
        if (grid.EditingControl is DataGridViewComboBoxEditingControl comboBox)
            comboBox.DroppedDown = true;
    }

    private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;
    }

    private void grid_CurrentCellChanged(object sender, EventArgs e)
    {
        // Defer: this can fire while the grid is still changing its current cell (e.g. removing a row).
        // Disabling a focused button there moves focus into the grid, which re-enters that change and throws.
        if (IsHandleCreated)
            BeginInvoke(UpdateButtons);
        else
            UpdateButtons();
    }

    private void grid_KeyDown(object sender, KeyEventArgs e)
    {
        bool isOpenMenuKey = e.KeyCode is Keys.Space or Keys.F4 || (e.Alt && e.KeyCode == Keys.Down);
        if (isOpenMenuKey && grid.CurrentCell?.ColumnIndex == ModifiersColumn.Index)
        {
            ShowModifiersMenu(grid.Rows[grid.CurrentCell.RowIndex]);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Delete && DeleteBtn.Enabled)
        {
            DeleteSelected();
            e.Handled = true;
        }
        else if (e.Alt && e.KeyCode == Keys.Up && UpBtn.Enabled)
        {
            MoveCurrent(-1);
            e.Handled = true;
        }
        else if (e.Alt && e.KeyCode == Keys.Down && DownBtn.Enabled)
        {
            MoveCurrent(1);
            e.Handled = true;
        }
    }

    private void AddBtn_Click(object sender, EventArgs e)
    {
        grid.EndEdit();

        IDEObjectProperty pro = new(GameObject);
        int i = 1;
        string name;
        do
        {
            name = "property" + i++;
        } while (!pro.IsValidName(name));
        pro.Name = name;

        GameObject.ExtraProperties.Add(pro);
        DataGridViewRow row = AddRow(pro);

        // select the new property and let the user type its name right away
        grid.ClearSelection();
        grid.Focus();
        grid.CurrentCell = row.Cells[NameColumn.Index];
        grid.BeginEdit(true);

        UpdateButtons();
        PropertiesChanged();
    }

    private void DeleteSelected()
    {
        grid.EndEdit();

        HashSet<DataGridViewRow> rows = [.. grid.SelectedCells.Cast<DataGridViewCell>().Select(cell => cell.OwningRow!)];
        if (grid.CurrentRow != null)
            rows.Add(grid.CurrentRow);

        foreach (var row in rows)
        {
            GameObject.ExtraProperties.Remove(PropertyOf(row));
            grid.Rows.Remove(row);
        }

        RevalidateNames();
        UpdateButtons();
        PropertiesChanged();
    }

    private void DeleteBtn_Click(object sender, EventArgs e) => DeleteSelected();

    private void MoveCurrent(int offset)
    {
        if (grid.CurrentCell == null)
            return;

        grid.EndEdit();

        int columnIndex = grid.CurrentCell.ColumnIndex;
        int oldIndex = grid.CurrentCell.RowIndex;
        int newIndex = oldIndex + offset;
        if (newIndex < 0 || newIndex >= grid.Rows.Count)
            return;

        DataGridViewRow row = grid.Rows[oldIndex];
        IDEObjectProperty pro = PropertyOf(row);

        // the order of the properties is the order of their initialization
        GameObject.ExtraProperties.RemoveAt(oldIndex);
        GameObject.ExtraProperties.Insert(newIndex, pro);

        grid.Rows.RemoveAt(oldIndex);
        grid.Rows.Insert(newIndex, row);

        grid.ClearSelection();
        grid.CurrentCell = row.Cells[columnIndex];

        UpdateButtons();
        PropertiesChanged();
    }

    private void UpBtn_Click(object sender, EventArgs e) => MoveCurrent(-1);

    private void DownBtn_Click(object sender, EventArgs e) => MoveCurrent(1);
}
