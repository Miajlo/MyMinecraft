namespace BinomialHeapLibrary;

//Min Binomial Heap at the moment, will support choosing between min or max version
public class BinomialHeap<T> where T : IComparable, new() {
    private NodeBinomial<T>? Head;
    public NodeBinomial<T>? Min { get; private set; }
    public int Size { get; set; }

    public BinomialHeap() {
        Size = 0;
        Head = Min = null;
    }

    public BinomialHeap(T data) {
        Size = 1;
        Head = Min = new(data);
    }

    public void Insert(T value) {
        if (Head == null) {
            Min = Head = new(value);
            ++Size;
            return;
        }

        BinomialHeap<T>? helper = new(value);

        HeapUnion(helper);

        helper.Head = helper.Min = null; // discard the helper heap
    }

    public void HeapUnion(BinomialHeap<T> heap2) {
        HeapMerge(heap2);

        NodeBinomial<T>? prev = null;
        NodeBinomial<T> current = Head!, next = Head!.Sibling!;

        while (next != null) {
            if (CheckUnionSkip(current, next)) {
                prev = current;
                current = next;
            }
            else {
                if (!Greater(current.Data, next.Data)) {
                    current.Sibling = next.Sibling;
                    next.Parent = current;
                    next.Sibling = current.Child;
                    current.Child = next;
                    current.Degree++;
                }
                else {
                    if (prev == null)
                        Head = next;
                    else
                        prev.Sibling = next;

                    current.Parent = next;
                    current.Sibling = next.Child;
                    next.Child = current;
                    next.Degree++;
                    current = next;
                }
            }
            next = current.Sibling!;
        }
    }

    public void HeapMerge(BinomialHeap<T> heap2) {
        NodeBinomial<T>? h1Iter = Head, h2Iter = heap2.Head, finalRootList = null, current = null;

        if (h1Iter == null || h2Iter == null)
            return;

        //if not initialized pick the smaller one to start the list
        //Not in loop to avoid unnecessary check for every iteration
        if (current == null) {
            if (h1Iter.Degree <= h2Iter.Degree) {
                current = h1Iter;
                h1Iter = h1Iter.Sibling;
            }
            else {
                current = h2Iter;
                h2Iter = h2Iter.Sibling;
            }
            finalRootList = current;
        }

        while (h1Iter != null && h2Iter != null) {
            if (h1Iter.Degree <= h2Iter.Degree) {
                current!.Sibling = h1Iter;
                current = current.Sibling;
                h1Iter = h1Iter.Sibling;
            }
            else {
                h2Iter.Parent = null;
                current!.Sibling = h2Iter;
                current = current.Sibling;
                h2Iter = h2Iter.Sibling;
            }
        }

        if (h1Iter != null)
            current!.Sibling = h1Iter;
        if (h2Iter != null && h2Iter.Parent == null)
            current!.Sibling = h2Iter;

        //In case of ExtractMin
        if (h2Iter != null && h2Iter.Parent != null) {
            while (h2Iter != null) {
                h2Iter.Parent = null;
                current!.Sibling = h2Iter;
                current = current.Sibling;
                h2Iter = h2Iter.Sibling;
            }
        }

        Size += heap2.Size;
        Min = Greater(Min!.Data, heap2.Min!.Data)
            ? heap2.Min
            : Min;
        Head = finalRootList;
        heap2.Head = heap2.Min = current = null;
        heap2.Size = 0;
    }

    private void FindMin() {
        NodeBinomial<T> tmp = Head!;

        while (tmp != null) {

            if (Min == null)
                Min = tmp;
            else if (!Greater(tmp.Data, Min!.Data))
                Min = tmp;

            tmp = tmp.Sibling!;
        }
    }

    private bool CheckUnionSkip(NodeBinomial<T> current, NodeBinomial<T> next) {
        return current.Degree != next.Degree ||
               (next.Sibling != null &&
               next.Sibling.Degree == current.Degree);
    }

    public T? ExtractMin() {

        if (Head == null)
            return default(T);

        NodeBinomial<T>? current = Head, previous = null;

        while (current != null && current != Min) {
            current = current.Sibling;
            previous = current;
        }

        if (previous == null)
            Head = current!.Sibling;
        else
            previous.Sibling = current!.Sibling;

        T? data = Min!.Data;
        Min = null;
        current = current!.Child;

        if (Head == null && current == null)
            Size = 0;
        else {
            if (Head == null && current != null) {
                Head = ReverseChildList(current);
                --Size;
            }
            else {
                if (Head != null && current == null)
                    --Size;
                else {
                    BinomialHeap<T> unionHeap = new();
                    unionHeap.Head = ReverseChildList(current);
                    HeapUnion(unionHeap);
                    --Size;
                }

            }
        }

        FindMin();

        return data;
    }

    public NodeBinomial<T>? ExtractMinNode() {
        if (Head == null)
            return null;

        NodeBinomial<T>? current = Head, previous = null;

        while (current != null && current != Min) {
            current = current.Sibling;
            previous = current;
        }

        if (previous == null)
            Head = current!.Sibling;
        else
            previous.Sibling = current!.Sibling;

        NodeBinomial<T>? retVal = current;
        Min = null;
        current = current!.Child;

        if (Head == null && current == null)
            Size = 0;
        else {
            if (Head == null && current != null) {
                Head = ReverseChildList(current);
                --Size;
            }
            else {
                if (Head != null && current == null)
                    --Size;
                else {
                    BinomialHeap<T> unionHeap = new();
                    unionHeap.Head = ReverseChildList(current);
                    HeapUnion(unionHeap);
                    --Size;
                }

            }
        }

        FindMin();

        return retVal;
    }

    private static NodeBinomial<T>? ReverseChildList(NodeBinomial<T>? node) {
        NodeBinomial<T>? tmp = null;

        while (node != null) {
            var helper = node.Sibling;
            node.Sibling = tmp;
            tmp = node;
            node = helper;
        }

        return tmp;
    }

    private bool Greater(T value1, T value2) {
        return value1.CompareTo(value2) == 1 ? true : false;
    }

    public void PrintRootList() {
        NodeBinomial<T>? tmp = Head;

        while (tmp != null) {
            Console.Write($"{tmp.Data} ");
            tmp = tmp.Sibling;
        }

        Console.WriteLine();
    }
}

