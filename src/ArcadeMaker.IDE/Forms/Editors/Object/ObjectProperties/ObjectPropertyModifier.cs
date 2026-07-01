using Exp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE.Editors.Object.ObjectProperties;

public partial class ObjectPropertyModifier : UserControl
{
    private readonly bool isInited = false;
    public IDEObjectProperty Property { get; }
    public ObjectPropertyModifier(IDEObjectProperty property)
    {
        this.Property = property;

        InitializeComponent();
        debugTimer.Tick += async (s, e) =>
        {
            // check for errors
            debugTimer.Stop();

            HashSet<ExpError>? errors = null;
            await Task.Run(() =>
            {
                Debugging.Debug.TryBuild();
            });
        };

        // disable type box changing selection on mouse wheel
        TypeBox.MouseWheel += (s, e) => ((HandledMouseEventArgs)e).Handled = true;

        // add items to type box
        foreach (var type in Enum.GetValues<VariableType>())
            TypeBox.Items.Add(type);

        // set values
        NameBox.Text = property.Name;
        TypeBox.SelectedItem = property.Type;
        InitValBox.Text = property.InitValueCode;
        ConstantBox.Checked = property.Constant;
        PrivateBox.Checked = property.Private;
        NullableBox.Checked = property.Nullable;

        isInited = true;
    }

    private void ObjectPropertyModifier_Load(object sender, EventArgs e)
    {

    }

    private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        VariableType selection = (VariableType)(TypeBox.SelectedItem ?? VariableType.Any);

        Property.Type = selection;

        if (selection == VariableType.Any)
        {
            NullableBox.Checked = true;
        }
        NullableBox.Enabled = selection != VariableType.Any;

        InitValBox.Text = IDEObjectProperty.GetDefaultInitVal(selection);
    }

    private readonly System.Windows.Forms.Timer debugTimer = new() { Interval = 3000 };
    private void InitValBox_TextChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        if (string.IsNullOrWhiteSpace(InitValBox.Text))
            InitValBox.BackColor = Color.Red;
        else
        {
            debugTimer.Stop();
            debugTimer.Start();

            Property.InitValueCode = InitValBox.Text;
            InitValBox.BackColor = Color.White;
        }
    }

    private void NameBox_TextChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        if (Property.IsValidName(NameBox.Text))
        {
            Property.Name = NameBox.Text;
            NameBox.BackColor = Color.White;
        }
        else
            NameBox.BackColor = Color.Red;
    }

    private void ConstantBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        Property.Constant = ConstantBox.Checked;
    }

    private void PrivateBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        Property.Private = PrivateBox.Checked;
    }

    private void NullableBox_CheckedChanged(object sender, EventArgs e)
    {
        if (!isInited)
            return;

        Property.Nullable = NullableBox.Checked;
    }
}

public class IDEObjectProperty(GameObject? obj) : Core.Resources.Serializeables.ObjectProperty
{
    [XmlIgnore]
    public GameObject? Object => obj;
    public IDEObjectProperty() : this(null) { } // for serializing

    public bool IsValidName(string name)
    {
        return Exp.Extensions.IsLiterallyValidName(name) && !Object.ExtraProperties.Any(p => p != this && p.Name == name) && !Core.Runtime.Instance.csProperties.Any(pair => pair.Key.Name.StartWithLowerCase() == name);
    }

    public static string GetDefaultInitVal(VariableType type)
    {
        if (type == VariableType.Bool)
            return "false";
        else if (type == VariableType.Char)
            return "'a'";
        else if (type == VariableType.Number)
            return "0";
        else if (type == VariableType.Array)
            return "[]";
        else if (type == VariableType.String)
            return "\"\"";
        else
            return "null";
    }
}