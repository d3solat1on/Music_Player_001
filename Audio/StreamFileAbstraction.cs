namespace QAMP.Audio;
public class StreamFileAbstraction(string name, System.IO.Stream readStream, System.IO.Stream writeStream) : TagLib.File.IFileAbstraction
{
    public string Name { get; } = name;
    public System.IO.Stream ReadStream { get; } = readStream;
    public System.IO.Stream WriteStream { get; } = writeStream;

    public void CloseStream(System.IO.Stream stream)
    {
        // Поток закроется сам при выходе из using в основном коде
    }
}