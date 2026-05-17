using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class GroupMembersApi
{
    private readonly ApiClient _api;

    public GroupMembersApi(ApiClient api) => _api = api;

    public Task<List<GroupMemberDto>> ListAsync(string groupId) =>
        _api.GetAsync<List<GroupMemberDto>>($"/api/groups/{groupId}/members");

    public Task<GroupMemberDto> InviteAsync(string groupId, string studentFullName) =>
        _api.PostAsync<GroupMemberDto>($"/api/groups/{groupId}/members", new InviteStudentRequest(studentFullName));
}
