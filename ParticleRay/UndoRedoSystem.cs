namespace ParticleRay;

public interface IUndoableAction
{
    void Execute();
    void Undo();
    string Description { get; }
}

public class UndoRedoSystem
{
    private readonly Stack<IUndoableAction> undoStack = new();
    private readonly Stack<IUndoableAction> redoStack = new();
    private const int MaxUndoStackSize = 50;

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public void ExecuteAction(IUndoableAction action)
    {
        action.Execute();
        undoStack.Push(action);
        redoStack.Clear(); // Clear redo stack when new action is performed
        
        // Limit undo stack size
        while (undoStack.Count > MaxUndoStackSize)
        {
            var oldestActions = undoStack.ToArray();
            undoStack.Clear();
            // Skip the oldest action and push the rest back
            for (int i = oldestActions.Length - 2; i >= 0; i--)
            {
                undoStack.Push(oldestActions[i]);
            }
        }
    }

    public void Undo()
    {
        if (!CanUndo) return;
        
        var action = undoStack.Pop();
        action.Undo();
        redoStack.Push(action);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        
        var action = redoStack.Pop();
        action.Execute();
        undoStack.Push(action);
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}

// Concrete action implementations
public class AddFrameAction : IUndoableAction
{
    private readonly AnimationSystem animationSystem;
    private readonly int frameIndex;
    private readonly Action<int> onFrameAdded;
    private readonly Action onFrameRemoved;

    public string Description => "Add Frame";

    public AddFrameAction(AnimationSystem animationSystem, int index, Action<int> onFrameAdded, Action onFrameRemoved)
    {
        this.animationSystem = animationSystem;
        this.frameIndex = index;
        this.onFrameAdded = onFrameAdded;
        this.onFrameRemoved = onFrameRemoved;
    }

    public void Execute()
    {
        animationSystem.InsertFrame(frameIndex);
        onFrameAdded?.Invoke(frameIndex);
    }

    public void Undo()
    {
        animationSystem.DeleteFrame(frameIndex);
        onFrameRemoved?.Invoke();
    }
}

public class DeleteFrameAction : IUndoableAction
{
    private readonly AnimationSystem animationSystem;
    private readonly Frame deletedFrame;
    private readonly int frameIndex;
    private readonly Action onFrameDeleted;
    private readonly Action<int> onFrameRestored;

    public string Description => "Delete Frame";

    public DeleteFrameAction(AnimationSystem animationSystem, int index, Action onFrameDeleted, Action<int> onFrameRestored)
    {
        this.animationSystem = animationSystem;
        this.frameIndex = index;
        this.onFrameDeleted = onFrameDeleted;
        this.onFrameRestored = onFrameRestored;
        // Store a copy of the frame before deletion
        var frame = animationSystem.GetFrame(index);
        this.deletedFrame = frame != null ? frame.Clone() : new Frame();
    }

    public void Execute()
    {
        animationSystem.DeleteFrame(frameIndex);
        onFrameDeleted?.Invoke();
    }

    public void Undo()
    {
        animationSystem.InsertFrame(frameIndex, deletedFrame);
        onFrameRestored?.Invoke(frameIndex);
    }
}

public class ClearFrameAction : IUndoableAction
{
    private readonly Frame frameBeforeClear;
    private readonly Frame frameToClear;
    private readonly int frameIndex;

    public string Description => "Clear Frame";

    public ClearFrameAction(Frame frame, int index)
    {
        this.frameToClear = frame;
        this.frameIndex = index;
        // Store a copy of the frame before clearing
        this.frameBeforeClear = frame.Clone();
    }

    public void Execute()
    {
        frameToClear.Clear();
    }

    public void Undo()
    {
        frameToClear.ParticleSnapshots.Clear();
        frameToClear.ParticleSnapshots.AddRange(frameBeforeClear.ParticleSnapshots);
        frameToClear.TrailSegments.Clear();
        frameToClear.TrailSegments.AddRange(frameBeforeClear.TrailSegments);
    }
}

public class DrawAction : IUndoableAction
{
    private readonly Frame frame;
    private readonly Frame frameStateBefore;
    private readonly Frame frameStateAfter;

    public string Description => "Draw";

    public DrawAction(Frame frame, Frame stateBefore, Frame stateAfter)
    {
        this.frame = frame;
        this.frameStateBefore = stateBefore.Clone();
        this.frameStateAfter = stateAfter.Clone();
    }

    public void Execute()
    {
        // Apply the after state
        frame.ParticleSnapshots.Clear();
        frame.ParticleSnapshots.AddRange(frameStateAfter.ParticleSnapshots);
        frame.TrailSegments.Clear();
        frame.TrailSegments.AddRange(frameStateAfter.TrailSegments);
    }

    public void Undo()
    {
        // Restore the before state
        frame.ParticleSnapshots.Clear();
        frame.ParticleSnapshots.AddRange(frameStateBefore.ParticleSnapshots);
        frame.TrailSegments.Clear();
        frame.TrailSegments.AddRange(frameStateBefore.TrailSegments);
    }
}