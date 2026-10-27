using EventsRestApi.Services.SharedContracts.Models;
using EventsRestApi.Services.UserService.Application.Exceptions;
using EventsRestApi.Services.UserService.Domain.Models.Entities;
using EventsRestApi.Services.UserService.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace EventsRestApi.Services.UserService.IntegrationTests
{
    public class UserRepositoryTests : BaseDbTest
    {
        private readonly DateTimeOffset fixedTime = new DateTimeOffset(2026, 06, 01, 09, 01, 01, TimeSpan.Zero);
        private readonly FakeTimeProvider _fakeTimeProvider;

        public UserRepositoryTests(DbFixture fixture) : base(fixture)
        {
            _fakeTimeProvider = new FakeTimeProvider();
            _fakeTimeProvider.SetUtcNow(fixedTime);
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnUserById()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var user = new User(
                new Guid($"A0000000-0000-0000-0000-000000000001"),
                $"User 1",
                $"Hashed Password 1",
                UserRole.User);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Users.AddAsync(user);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.GetByIdAsync(user.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(result);
            Assert.Equivalent(user, result, true);
        }


        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnNull()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.GetByIdAsync(Guid.Empty, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        #endregion

        #region GetByLoginAsync Tests

        [Fact]
        public async Task GetByLoginAsync_WithExistingLogin_ReturnUserByLogin()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var user = new User(
                new Guid($"A0000000-0000-0000-0000-000000000001"),
                $"User 1",
                $"Hashed Password 1",
                UserRole.User);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Users.AddAsync(user);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.GetByLoginAsync(user.Login, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(result);
            Assert.Equivalent(user, result, true);
        }


        [Fact]
        public async Task GetByLoginAsync_WithNonExistentLogin_ReturnNull()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.GetByLoginAsync("NonExistentLogin", CancellationToken.None);

            /*---ASSERT---*/
            Assert.Null(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_EverythingIsCorrect_ReturnAddedUser()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var user = new User(
                new Guid($"A0000000-0000-0000-0000-000000000001"),
                $"User 1",
                $"Hashed Password 1",
                UserRole.User);

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.AddAsync(user.Login, user.HashedPassword, user.Role, CancellationToken.None);

            /*---ASSERT---*/
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(user, options =>
                options.Excluding(x => x.Id));
        }


        [Fact]
        public async Task AddAsync_WithExistingLogin_ThrowDbUpdateException()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var user = new User(
                new Guid($"A0000000-0000-0000-0000-000000000001"),
                $"User 1",
                $"Hashed Password 1",
                UserRole.User);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Users.AddAsync(user);
            await arrangeContext.SaveChangesAsync();

            /*---ACT & ASSERT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);

            await Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
                await userRepository.AddAsync(user.Login, $"{user.HashedPassword} new", user.Role, CancellationToken.None));
        }

        #endregion
    }
}
