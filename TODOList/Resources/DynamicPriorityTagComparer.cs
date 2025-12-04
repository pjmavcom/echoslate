using System.Collections;

namespace TODOList.Resources;

public class DynamicPriorityTagComparer : IComparer {
    private readonly string _priorityTag;

    public DynamicPriorityTagComparer(string priorityTag) => _priorityTag = priorityTag;

    public int Compare(object? x, object? y) {
        if (x is not TodoItemHolder a || y is not TodoItemHolder b) {
            return 0;
        }
        bool aHasPriority = a.Tags?.Contains(_priorityTag) == true;
        bool bHasPriority = b.Tags?.Contains(_priorityTag) == true;

// Case 1: One has priority, the other doesn't → priority wins
        if (aHasPriority && !bHasPriority) return -1;
        if (!aHasPriority && bHasPriority) return 1;

        // Case 2: Both have priority OR both don't → let SortDescriptions handle it
        //         (we return 0 so secondary sorting kicks in)
        return 0;

    }
}
