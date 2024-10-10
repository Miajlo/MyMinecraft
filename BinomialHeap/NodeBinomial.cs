namespace BinomialHeapLibrary;

public class NodeBinomial<T> where T : IComparable, new() {
    #region Properties
    public T Data { get; set; }
    public int Degree { get; set; }
    public NodeBinomial<T>? Sibling { get; set; }
    public NodeBinomial<T>? Child { get; set; }
    public NodeBinomial<T>? Parent { get; set; }
    #endregion
    public NodeBinomial() {
        Sibling = Child = Parent = null;
        Degree = 0;
        Data = new();
    }
    public NodeBinomial(T data) : this() {
        Data = data;
    }
    public NodeBinomial(T data, NodeBinomial<T>? sibling = null, NodeBinomial<T>? parent = null, NodeBinomial<T>? child = null)
        : this(data) {
        Sibling = sibling;
        Parent = parent;
        Child = child;
    }

    public NodeBinomial<T>? RevertSiblingList(NodeBinomial<T>? node) {
        NodeBinomial<T>? helper;

        if (Sibling == null)
            helper = this;
        else
            helper = Sibling!.RevertSiblingList(this);

        Parent = null;
        Sibling = node;

        return helper;
    }

    public void LinkChild(NodeBinomial<T> child) {

    }
}
