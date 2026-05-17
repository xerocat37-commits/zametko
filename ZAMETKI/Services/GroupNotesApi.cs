using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class GroupNotesApi
{
    private readonly ApiClient _api;

    public GroupNotesApi(ApiClient api) => _api = api;

    public Task<List<NoteDto>> ListAsync(string groupId) =>
        _api.GetAsync<List<NoteDto>>($"/api/groups/{groupId}/notes");

    public Task<NoteDto> CreateAsync(string groupId, string title, string content) =>
        _api.PostAsync<NoteDto>($"/api/groups/{groupId}/notes", new CreateNoteRequest(title, content));
}
