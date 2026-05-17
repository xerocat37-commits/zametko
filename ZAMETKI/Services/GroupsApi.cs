using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class GroupsApi
{
    private readonly ApiClient _api;

    public GroupsApi(ApiClient api) => _api = api;

    public Task<List<GroupDto>> GetMyAsync() =>
        _api.GetAsync<List<GroupDto>>("/api/groups/my");

    public Task<GroupDto> CreateAsync(string name) =>
        _api.PostAsync<GroupDto>("/api/groups", new CreateGroupRequest(name));

    public Task<GroupDto> GetByIdAsync(string id) =>
        _api.GetAsync<GroupDto>($"/api/groups/{id}");
}
