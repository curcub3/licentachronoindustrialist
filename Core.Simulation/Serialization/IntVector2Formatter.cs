using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Core.Simulation.Data;

namespace Core.Simulation.Serialization
{
    public class IntVector2Formatter : IMessagePackFormatter<IntVector2>
    {
        public void Serialize(ref MessagePackWriter writer, IntVector2 value, MessagePackSerializerOptions options)
        {
            // FixArray length 2: 0x92 header + 8 bytes = ~9 bytes per IntVector2
            writer.WriteArrayHeader(2);
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public IntVector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                throw new MessagePackSerializationException("IntVector2 cannot be nil.");

            int count = reader.ReadArrayHeader();
            if (count != 2)
                throw new MessagePackSerializationException("Invalid IntVector2 array length.");

            return new IntVector2(reader.ReadInt32(), reader.ReadInt32());
        }
    }

    public static class ChronosSerialization
    {
        public static readonly IFormatterResolver Resolver;
        public static readonly MessagePackSerializerOptions Options;

        static ChronosSerialization()
        {
            Resolver = CompositeResolver.Create(
                new IMessagePackFormatter[] { new IntVector2Formatter() },
                new IFormatterResolver[] { StandardResolver.Instance }
            );
            Options = MessagePackSerializerOptions.Standard.WithResolver(Resolver);
        }
    }
}
