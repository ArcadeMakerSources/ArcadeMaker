using System;
using System.Collections.ObjectModel;
using System.Drawing;
using DrawingOperation = System.Action<System.Drawing.Graphics>;

namespace ArcadeMaker.IDE;

partial class SpriteDesigner : Form, IDisposable
{
    private readonly Bitmap originalImage;
    private readonly Bitmap currentImage;
    private readonly Dictionary<Button, DrawingTool> toolbox = [];
    private DrawingTool selectedTool = null!;

    // draw params
    private FillTypes fillType = FillTypes.Outline;
    private int thickness = 1;

    // draw variables
    private readonly ObservableCollection<DrawingOperation> drawingOperations = [];
    private readonly Stack<DrawingOperation> redoDrawingOperations = [];
    private Point mouseDownPosition;
    private System.Windows.Forms.MouseButtons pressedButton = MouseButtons.None;
    private DrawingOperation? previewDrawingOperation = null;
    private readonly List<DrawingOperation> onTheMoveRecord = [];

    // view settings
    private const float ZOOM_DIFF = 0.2f;
    private float zoom = 1;

    private enum FillTypes
    {
        Outline,
        Fill,
        FillAndOutline
    }

    public SpriteDesigner(Bitmap image)
    {
        InitializeComponent();

        originalImage = (image ?? throw new ArgumentNullException(nameof(image))).Copy();
        currentImage = originalImage;

        // init toolbox
        InitToolbox();
        ToolButton_Click(rectBtn, EventArgs.Empty); // select any tool so that selectedTool won't be null
        if (selectedTool == null)
            throw new Exception($"Field '{nameof(selectedTool)}' cannot be null.");

        // init image view
        imageBox.Image = currentImage;
        imageBox.Size = currentImage.Size;

        drawingOperations.CollectionChanged += (s, e) => SetUndoRedoEnabled();
    }

    private void imageBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Middle)
            return;

        pressedButton = e.Button;
        mouseDownPosition = e.Location;
        onTheMoveRecord.Clear();

        if (selectedTool.OnMouseDown)
        {
            Draw(e.Location, false);
        }
    }

    private void imageBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (pressedButton == MouseButtons.None)
            return;

        if (!selectedTool.DisablePreview)
        {
            previewDrawingOperation = Draw(e.Location, true);

            if (selectedTool.OnTheMove)
            {
                // record the full move as 1 drawing
                onTheMoveRecord.Add(previewDrawingOperation);
            }
        }

        imageBox.Invalidate();
    }

    private void imageBox_MouseUp(object sender, MouseEventArgs e)
    {
        if (pressedButton == MouseButtons.None)
            return;

        if (selectedTool.OnTheMove)
        {
            drawingOperations.Add(JoinDrawingOperations(onTheMoveRecord.ToArray() /* keep ToArray(), otherwise the drawing will disappear on Clear()!! */));
            imageBox.Invalidate();
        }
        else if (selectedTool.OnMouseUp)
            Draw(e.Location, false);

        pressedButton = MouseButtons.None;
        onTheMoveRecord.Clear(); // we already do it in MouseDown, but doing it here too won't be harmful
    }

    private DrawingOperation Draw(Point mouseUpPosition, bool preview)
    {
        // deconstruct positions
        int x1 = mouseDownPosition.X, y1 = mouseDownPosition.Y;
        int x2 = mouseUpPosition.X, y2 = mouseUpPosition.Y;

        // remove zoom
        x1 = (int)(x1 / zoom);
        y1 = (int)(y1 / zoom);
        x2 = (int)(x2 / zoom);
        y2 = (int)(y2 / zoom);

        // get colors
        Color col1 = pressedButton == MouseButtons.Left ? colorPicker.Color1 : colorPicker.Color2;
        Color col2 = pressedButton == MouseButtons.Left ? colorPicker.Color2 : colorPicker.Color1;

        // draw
        DrawingOperation op = selectedTool.Draw(col1, col2, new(x1, y1), new(x2, y2), fillType, thickness);
        if (!preview)
        {
            redoDrawingOperations.Clear();
            drawingOperations.Add(op);
            imageBox.Invalidate();
        }

        return op;
    }

    private void DrawResult(Graphics graphics, DrawingOperation? preview = null)
    {
        foreach (var op in drawingOperations)
            op(graphics);
        preview?.Invoke(graphics);
    }

    public Bitmap GetResult()
    {
        Bitmap bmp = new(originalImage.Width, originalImage.Height);
        using (Graphics graphics = Graphics.FromImage(bmp))
        {
            graphics.DrawImage(originalImage, 0, 0);
            DrawResult(graphics);
        }
        return bmp;
    }

    public static DrawingOperation JoinDrawingOperations(params IEnumerable<DrawingOperation> drawingOperations) => graphics =>
    {
        foreach (DrawingOperation op in drawingOperations)
            op(graphics);
    };

    private static void DrawBackground(Graphics graphics, int viewWidth, int viewHeight)
    {
        const int size = 16;
        bool col = false;
        for (int x = 0; x < viewWidth; x += size)
        {
            for (int y = 0; y < viewHeight; y += size)
            {
                graphics.FillRectangle(col ? Brushes.White : Brushes.Gray, x, y, size, size);
                col = !col;
            }
            col = (x / size) % 2 == 0;
        }
    }

    private void DrawGrid(Graphics graphics, int imageWidth, int imageHeight)
    {
        return;
        float viewWidth = imageWidth * zoom, viewHeight = imageHeight * zoom;

        if (imageWidth >= viewWidth || imageHeight >= viewHeight)
            return;

        for (float x = 0; x < viewWidth; x += zoom)
        {
            for (float y = 0; y < viewHeight; y += zoom)
            {
                graphics.DrawLine(Pens.Gray, x, y, x + zoom, y);
                graphics.DrawLine(Pens.Gray, x, y, x, y + zoom);
            }
        }
    }

    public new void Dispose()
    {
        base.Dispose();

        // TODO: delete originalImage or dispose it too
        currentImage.Dispose();
    }

    private void InitToolbox()
    {
        toolbox.Add(rectBtn, new RectangleDrawer());
        toolbox.Add(ellipseBtn, new EllipseDrawer());
        toolbox.Add(lineBtn, new LineDrawer());
        toolbox.Add(penBtn, new PenTool());
        toolbox.Add(fillBtn, new BucketTool(GetResult));

        // subscribe buttons to listener
        foreach (Button btn in toolbox.Keys)
            btn.Click += ToolButton_Click;
    }

    private void ToolButton_Click(object? sender, EventArgs e)
    {
        // make sure it's a button click
        if (sender is not Button btn)
            return;

        // make sure this button has a tool pair
        if (!toolbox.TryGetValue(btn, out DrawingTool? tool))
            return;

        selectedTool = tool;
    }

    private void width1Btn_Click(object sender, EventArgs e)
    {
        thickness = 1;
    }

    private void width2Btn_Click(object sender, EventArgs e)
    {
        thickness = 2;
    }

    private void width3Btn_Click(object sender, EventArgs e)
    {
        thickness = 3;
    }

    private void width4Btn_Click(object sender, EventArgs e)
    {
        thickness = 4;
    }

    private void width5Btn_Click(object sender, EventArgs e)
    {
        thickness = 5;
    }

    private void width6Btn_Click(object sender, EventArgs e)
    {
        thickness = 6;
    }

    private void shapeDrawBtn_Click(object sender, EventArgs e)
    {
        fillType = FillTypes.Outline;
    }

    private void shapeDrawFillBtn_Click(object sender, EventArgs e)
    {
        fillType = FillTypes.FillAndOutline;
    }

    private void shapeFillBtn_Click(object sender, EventArgs e)
    {
        fillType = FillTypes.Fill;
    }

    private void okBtn_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void imageBox_Paint(object sender, PaintEventArgs e)
    {
        if (imageBox.Image == null)
            return;

        DrawBackground(e.Graphics, imageBox.Size.Width, imageBox.Size.Height);

        e.Graphics.DrawImage(imageBox.Image, 0, 0, imageBox.Size.Width, imageBox.Size.Height);

        e.Graphics.ScaleTransform(zoom, zoom);
        DrawResult(e.Graphics, previewDrawingOperation);

        // draw onTheMoveRecord
        if (pressedButton != MouseButtons.None)
        {
            foreach (DrawingOperation op in onTheMoveRecord)
                op(e.Graphics);
        }

        e.Graphics.ResetTransform();

        DrawGrid(e.Graphics, imageBox.Image.Width, imageBox.Image.Height);

        previewDrawingOperation = null;
    }

    private void Undo()
    {
        if (drawingOperations.Count == 0)
            return;

        DrawingOperation undo = drawingOperations.Last();
        redoDrawingOperations.Push(undo);
        drawingOperations.Remove(undo);
        imageBox.Invalidate();
    }

    private void Redo()
    {
        if (redoDrawingOperations.Count == 0)
            return;

        drawingOperations.Add(redoDrawingOperations.Pop());
        imageBox.Invalidate();
    }

    private void undoBtn_Click(object sender, EventArgs e) => Undo();

    private void redoBtn_Click(object sender, EventArgs e) => Redo();

    private void SetUndoRedoEnabled()
    {
        undoBtn.Enabled = drawingOperations.Count >= 1;
        redoBtn.Enabled = redoDrawingOperations.Count >= 1;
    }

    private void zoomInBtn_Click(object sender, EventArgs e)
    {
        if (imageBox.Image == null)
            return;

        zoom += ZOOM_DIFF;
        SetViewSize();
    }

    private void zoomOutBtn_Click(object sender, EventArgs e)
    {
        if (imageBox.Image == null)
            return;

        zoom -= ZOOM_DIFF;
        SetViewSize();
    }

    private void SetViewSize()
    {
        if (imageBox.Image == null)
            return;

        imageBox.Size = new((int)(imageBox.Image.Size.Width * zoom), (int)(imageBox.Image.Size.Height * zoom));
    }
}