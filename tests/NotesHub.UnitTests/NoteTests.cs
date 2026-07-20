using NotesHub.Domain;
namespace NotesHub.UnitTests;
public sealed class NoteTests { [Fact] public void Update_requires_a_title() { var note = new Note { OwnerId = "user" }; Assert.Throws<ArgumentException>(() => note.Update("", "", false)); } }
