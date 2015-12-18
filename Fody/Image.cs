//
// Image.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2009 - 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
public static class Image
{
    static bool Advance(this Stream stream, int length)
    {
        if (stream.Position + length >= stream.Length)
        {
            return false;
        }

        stream.Seek(length, SeekOrigin.Current);
        return true;
    }

    static bool MoveTo(this Stream stream, uint position)
    {
        if (position >= stream.Length)
        {
            return false;
        }

        stream.Position = position;
        return true;
    }

    static ushort ReadUInt16(this Stream stream)
    {
        return (ushort)(stream.ReadByte()
                        | (stream.ReadByte() << 8));
    }

    static uint ReadUInt32(this Stream stream)
    {
        return (uint)(stream.ReadByte()
                      | (stream.ReadByte() << 8)
                      | (stream.ReadByte() << 16)
                      | (stream.ReadByte() << 24));
    }

    public static bool IsAssembly(string file)
    {
        using (var stream = File.OpenRead(file))
        {
            if (stream.Length < 318)
            {
                return false;
            }
            if (stream.ReadUInt16() != 0x5a4d)
            {
                return false;
            }
            if (!stream.Advance(58))
            {
                return false;
            }
            if (!stream.MoveTo(stream.ReadUInt32()))
            {
                return false;
            }
            if (stream.ReadUInt32() != 0x00004550)
            {
                return false;
            }
            if (!stream.Advance(20))
            {
                return false;
            }
            if (!stream.Advance(stream.ReadUInt16() == 0x20b ? 222 : 206))
            {
                return false;
            }

            return stream.ReadUInt32() != 0;
        }
    }
}

