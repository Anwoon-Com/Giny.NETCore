using Giny.IO.D2O;
using System.Collections;

class Program
{
    // Giny D2O editor.
    //   dump <file.d2o> [index]      -> prints objects and their fields
    //   set <file.d2o> <index> <field> <value>  -> updates a field
    //   del <file.d2o> <index>       -> deletes an object
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: dump|set|del <file.d2o> [index] [field] [value]");
            return 1;
        }
        switch (args[0])
        {
            case "dump": Dump(args[1], args.Length > 2 ? int.Parse(args[2]) : -1); return 0;
            case "set": Set(args[1], int.Parse(args[2]), args[3], args[4]); return 0;
            case "del": Del(args[1], int.Parse(args[2])); return 0;
            default: Console.WriteLine("unknown command " + args[0]); return 1;
        }
    }
    static void Dump(string file, int index)
    {
        var reader = new D2OReader(file);
        var objects = reader.ReadObjects(true);
        foreach (var entry in objects)
        {
            if (index >= 0 && entry.Key != index) continue;
            Console.WriteLine("[" + entry.Key + "]");
            foreach (var field in Fields(entry.Value))
                Console.WriteLine("  " + field);
        }
    }
    static void Set(string file, int index, string field, string value)
    {
        var objects = new D2OReader(file).ReadObjects(true);
        var obj = objects[index];
        SetField(obj, field, value);
        using (var writer = new D2OWriter(file))
        {
            writer.StartWriting();
            writer.Write(obj, index);
            writer.EndWriting();
        }
        Console.WriteLine(index + "." + field + " = " + value);
    }
    static void Del(string file, int index)
    {
        using (var writer = new D2OWriter(file))
        {
            writer.StartWriting();
            writer.Delete(index);
            writer.EndWriting();
        }
        Console.WriteLine("deleted " + index);
    }
    static IEnumerable<string> Fields(object obj)
    {
        if (obj is IDictionary dict)
            foreach (DictionaryEntry e in dict) yield return e.Key + " = " + e.Value;
        else
        {
            foreach (var f in obj.GetType().GetFields())
            {
                object v;
                try { v = f.GetValue(obj); } catch { v = "?"; }
                yield return f.Name + " = " + Format(v);
            }
            foreach (var p in obj.GetType().GetProperties())
            {
                object v;
                try { v = p.GetValue(obj); } catch { continue; }
                yield return p.Name + " = " + Format(v);
            }
        }
    }
    static string Format(object v)
    {
        if (v is System.Collections.IEnumerable en && !(v is string))
        {
            var parts = new System.Text.StringBuilder();
            foreach (var e in en) parts.Append(e).Append(',');
            return parts.ToString().TrimEnd(',');
        }
        return v == null ? "" : v.ToString();
    }
    static void SetField(object obj, string field, string value)
    {
        if (obj is IDictionary dict) { dict[field] = Coerce(value, dict[field]); return; }
        var prop = obj.GetType().GetProperty(field);
        prop.SetValue(obj, System.Convert.ChangeType(value, prop.PropertyType));
    }
    static object Coerce(string value, object old)
    {
        if (old == null) return value;
        return System.Convert.ChangeType(value, old.GetType());
    }
}
