using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignment2.LibraryManagement
{
    /// <summary>
    /// Custom JSON converter for polymorphic Item serialization
    /// Handles Book, Music, and Toy types with a $type discriminator
    /// </summary>
    public class ItemJsonConverter : JsonConverter<Item>
    {
        private const string TypePropertyName = "$type";

        /// <summary>
        /// Read (deserialize) an Item from JSON
        /// Uses $type discriminator to determine actual item type
        /// </summary>
        public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // We need to read the entire element to examine its content
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;

                // Get the type discriminator
                if (!root.TryGetProperty(TypePropertyName, out JsonElement typeElement))
                {
                    throw new JsonException("Item JSON must contain a '$type' discriminator property");
                }

                string itemType = typeElement.GetString();

                // Extract common properties
                string name = root.GetProperty(nameof(Item.Name)).GetString() ?? string.Empty;
                string description = root.GetProperty(nameof(Item.Description)).GetString() ?? string.Empty;
                string libraryCode = root.GetProperty(nameof(Item.LibraryCode)).GetString() ?? string.Empty;
                string status = root.GetProperty(nameof(Item.Status)).GetString() ?? "Available";
                string branchId = root.TryGetProperty(nameof(Item.BranchId), out JsonElement branchElement) 
                    ? branchElement.GetString() ?? "DEFAULT" 
                    : "DEFAULT";

                // Create the appropriate item type based on discriminator
                return itemType switch
                {
                    "Book" => DeserializeBook(root, name, description, libraryCode, status, branchId),
                    "Music" => DeserializeMusic(root, name, description, libraryCode, status, branchId),
                    "Toy" => DeserializeToy(root, name, description, libraryCode, status, branchId),
                    _ => throw new JsonException($"Unknown item type: {itemType}")
                };
            }
        }

        /// <summary>
        /// Write (serialize) an Item to JSON
        /// Adds $type discriminator and item-specific properties
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write the type discriminator
            string typeName = value.GetType().Name;
            writer.WriteString(TypePropertyName, typeName);

            // Write common properties
            writer.WriteString(nameof(Item.Name), value.Name);
            writer.WriteString(nameof(Item.Description), value.Description);
            writer.WriteString(nameof(Item.LibraryCode), value.LibraryCode);
            writer.WriteString(nameof(Item.Status), value.Status);
            writer.WriteString(nameof(Item.BranchId), value.BranchId);

            // Write type-specific properties
            if (value is Book book)
            {
                writer.WriteString(nameof(Book.Author), book.Author);
                writer.WriteString(nameof(Book.Genre), book.Genre);
            }
            else if (value is Music music)
            {
                writer.WriteString(nameof(Music.Artist), music.Artist);
                writer.WriteNumber(nameof(Music.Year), music.Year);
            }
            else if (value is Toy toy)
            {
                writer.WriteString(nameof(Toy.Type), toy.Type);
                writer.WriteString(nameof(Toy.AgeGroup), toy.AgeGroup);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Deserialize a Book item from JSON element
        /// </summary>
        private Book DeserializeBook(JsonElement root, string name, string description, string libraryCode, string status, string branchId)
        {
            string author = root.GetProperty(nameof(Book.Author)).GetString() ?? string.Empty;
            string genre = root.GetProperty(nameof(Book.Genre)).GetString() ?? string.Empty;

            return new Book(name, description, libraryCode, status, author, genre, branchId);
        }

        /// <summary>
        /// Deserialize a Music item from JSON element
        /// </summary>
        private Music DeserializeMusic(JsonElement root, string name, string description, string libraryCode, string status, string branchId)
        {
            string artist = root.GetProperty(nameof(Music.Artist)).GetString() ?? string.Empty;
            int year = root.GetProperty(nameof(Music.Year)).GetInt32();

            return new Music(name, description, libraryCode, status, artist, year, branchId);
        }

        /// <summary>
        /// Deserialize a Toy item from JSON element
        /// </summary>
        private Toy DeserializeToy(JsonElement root, string name, string description, string libraryCode, string status, string branchId)
        {
            string toyType = root.GetProperty(nameof(Toy.Type)).GetString() ?? string.Empty;
            string ageGroup = root.GetProperty(nameof(Toy.AgeGroup)).GetString() ?? string.Empty;

            return new Toy(name, description, libraryCode, status, toyType, ageGroup, branchId);
        }
    }
}

