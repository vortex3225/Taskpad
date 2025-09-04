using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Taskpad.Objects;

namespace Taskpad.Scripts
{
    public class Snapshot
    {
        public List<TaskObject> SnapshottedTaskList { get; set; } = null;
    }

    public static class SnapshotService
    {
        private static MainWindow ?window = null;

        private static Stack<Snapshot> UndoStack = new Stack<Snapshot>();
        private static Stack<Snapshot> RedoStack = new Stack<Snapshot>();

        private const int MAX_SNAPSHOTS = 5; // number of snapshots allowed before the stack is consolidated

        public static void Init(MainWindow created_window)
        {
            window = created_window;
        }

        public static void CreateSnapshot(List<TaskObject> tasks)
        {
            if (window == null)
                return;
            ConsolidateStacks();

            Snapshot new_snapshot = new Snapshot
            {
                SnapshottedTaskList = tasks.Select(t => new TaskObject
                {
                    Name = t.Name,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Completed = t.Completed
                }).ToList()
            };
            UndoStack.Push(new_snapshot);
        }

        public static void ClearHistory()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public static void AddRedoPoint(List<TaskObject> tasks)
        {
            ConsolidateStacks();

            Snapshot snapshot = new Snapshot
            {
                SnapshottedTaskList = tasks.Select(t => new TaskObject
                {
                    Name = t.Name,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Completed = t.Completed
                }).ToList()
            };
            RedoStack.Push(snapshot);
        }

        private static void ConsolidateStacks()
        {
            List<Snapshot> GetListFromStack(Stack<Snapshot> target_Stack)
            {
                return target_Stack.ToList();
            }

            void Consolidate(List<Snapshot> list, string stack_name = "und0")
            {
                while (list.Count > MAX_SNAPSHOTS)
                {
                    list.RemoveAt(list.Count - 1);
                }
                switch (stack_name)
                {
                    case "undo":
                        UndoStack = new Stack<Snapshot>(list);
                        break;
                    case "redo":
                        RedoStack = new Stack<Snapshot>(list);
                        break;
                    default:
                        break;
                }
            }

            if (UndoStack.Count > MAX_SNAPSHOTS)
            {
                List<Snapshot> snapshots = GetListFromStack(UndoStack);
                UndoStack.Clear();
                Consolidate(snapshots);
            }
            else if (RedoStack.Count  > MAX_SNAPSHOTS)
            {
                List<Snapshot> snapshots = GetListFromStack(RedoStack);
                RedoStack.Clear();
                Consolidate(snapshots, "redo");
            }
        }

        public static void Undo()
        {
            if (window == null || UndoStack.Count == 0)
                return;

            Snapshot snapshotToUndo = UndoStack.Pop();
            List<TaskObject> currentTasks = window.GetCurrentTasks(); // make deep copy
            AddRedoPoint(currentTasks);
            window.LoadSnapshot(snapshotToUndo);
        }

        public static void Redo()
        {
            if (window == null || RedoStack.Count == 0)
                return;

            Snapshot snapshotToRedo = RedoStack.Pop();
            UndoStack.Push(window.GetCurrentTasksAsSnapshot());
            window.LoadSnapshot(snapshotToRedo, true);
        }
    }
}