using Godot;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Core.Simulation.Serialization;
using Client.Localization;

namespace Client.Boot
{
	// Place this as an AutoLoad (Singleton) in Godot Project Settings
	// so it runs before everything else and registers serialization options globally.
	public partial class Startup : Node
	{
		public override void _EnterTree()
		{
			Localizer.Initialize();
			InitializeSerialization();
		}

		private void InitializeSerialization()
		{
			GD.Print("Startup: Initializing MessagePack AOT Resolvers...");

			// Composite resolver:
			// 1. Custom Phase 2 formatters (IntVector2)
			// 2. StandardResolver for built-in types
			// Note: Add GeneratedResolver.Instance here if the MessagePack Source Generator
			// is configured in the project for full AOT compatibility.
			var resolver = CompositeResolver.Create(
				new IMessagePackFormatter[] {
					new IntVector2Formatter()
				},
				new IFormatterResolver[] {
					StandardResolver.Instance
				}
			);

			// LZ4 compression keeps save files small
			var options = MessagePackSerializerOptions.Standard
				.WithResolver(resolver)
				.WithCompression(MessagePackCompression.Lz4BlockArray);

			// Set as global default so the rest of the app doesn't need to pass options
			MessagePackSerializer.DefaultOptions = options;

			GD.Print("Startup: AOT Serialization Registry Complete.");
		}
	}
}
