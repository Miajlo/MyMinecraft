﻿namespace MyMinecraft.Graphics; 
public class IBO :IDisposable {
    public int ID;

    public IBO(List<uint> data) {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Count * sizeof(uint),
                      data.ToArray(), BufferUsageHint.StreamDraw);
    }

    public void Bind() {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
    }
    public void Unbind() {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Delete() {
        GL.DeleteBuffer(ID);
    }

    public void Dispose() {
        Delete();
        GC.SuppressFinalize(this);
    }
}
