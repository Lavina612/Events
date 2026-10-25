using EventsRestApi.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Domain.Models.Entities;
using EventsRestApi.Infrastructure.Data;
using EventsRestApi.Infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace EventsRestApi.IntegrationsTests
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
            result.Should().BeEquivalentTo(user, options =>
                options.Excluding(x => x.Bookings));
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
            result.Should().BeEquivalentTo(user, options =>
                options.Excluding(x => x.Bookings));
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
                options.Excluding(x => x.Id)
                    .Excluding(x => x.Bookings));
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

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
                await userRepository.AddAsync(user.Login, $"{user.HashedPassword} new", user.Role, CancellationToken.None));
        }

        #endregion

        #region GetActiveBookingsCountAsync Tests

        [Fact]
        public async Task GetActiveBookingsCountAsync_EverythingIsCorrect_ReturnRealCount()
        {
            /*---ARRANGE---*/
            await ResetDatabaseAsync();

            var user1 = new User(
                new Guid($"A0000000-0000-0000-0000-000000000001"),
                $"User 1",
                $"Hashed Password 1",
                UserRole.User);

            var user2 = new User(
                new Guid($"A0000000-0000-0000-0000-000000000002"),
                $"User 2",
                $"Hashed Password 2",
                UserRole.User);

            var @event = new Event(
                new Guid("E0000000-0000-0000-0000-000000000001"),
                "Event 1",
                "Description 1",
                fixedTime.UtcDateTime.AddDays(-5),
                fixedTime.UtcDateTime.AddDays(5),
                1);

            var booking1 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000001"),
                @event.Id,
                user2.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime);

            var booking2 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000002"),
                @event.Id,
                user1.Id,
                BookingStatus.Pending,
                1,
                fixedTime.UtcDateTime);

            var booking3 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000003"),
                @event.Id,
                user1.Id,
                BookingStatus.Confirmed,
                1,
                fixedTime.UtcDateTime);

            var booking4 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000004"),
                @event.Id,
                user1.Id,
                BookingStatus.Cancelled,
                1,
                fixedTime.UtcDateTime);

            var booking5 = new Booking(
                new Guid("B0000000-0000-0000-0000-000000000005"),
                @event.Id,
                user1.Id,
                BookingStatus.Rejected,
                1,
                fixedTime.UtcDateTime);

            await using var arrangeContext = CreateContext();
            await arrangeContext.Users.AddAsync(user1);
            await arrangeContext.Users.AddAsync(user2);
            await arrangeContext.Events.AddAsync(@event);
            await arrangeContext.Bookings.AddAsync(booking1);
            await arrangeContext.Bookings.AddAsync(booking2);
            await arrangeContext.Bookings.AddAsync(booking3);
            await arrangeContext.Bookings.AddAsync(booking4);
            await arrangeContext.Bookings.AddAsync(booking5);
            await arrangeContext.SaveChangesAsync();

            /*---ACT---*/
            await using var actContext = CreateContext();
            var userRepository = new UserRepository(actContext);
            var result = await userRepository.GetActiveBookingsCountAsync(user1.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(2, result);
        }


        [Fact]
        public async Task GetActiveBookingsCountAsync_NoBookingsInDb_ReturnZero()
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
            var result = await userRepository.GetActiveBookingsCountAsync(user.Id, CancellationToken.None);

            /*---ASSERT---*/
            Assert.Equal(0, result);
        }

        #endregion
    }
}
