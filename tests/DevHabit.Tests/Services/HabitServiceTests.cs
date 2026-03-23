using System.Linq.Expressions;
using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Application.Contracts.Repositories;
using DevHabit.Application.DTOs.Habits;
using DevHabit.Application.Interfaces;
using DevHabit.Application.Services;
using DevHabit.Domain.Entities;
using DevHabit.Domain.Enums;
using Moq;

namespace DevHabit.Tests.Services;

public class HabitServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly RequestScopedService _scopedService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly HabitService _habitService;

    public HabitServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _scopedService = new RequestScopedService { UserId = "user123" };
        _mockFileService = new Mock<IFileService>();
        _mockCacheService = new Mock<ICacheService>();

        _habitService = new HabitService(
            _mockUnitOfWork.Object,
            _scopedService,
            _mockFileService.Object,
            _mockCacheService.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateHabit_WhenValidDto()
    {
        // Arrange
        var createHabitDto = new CreateHabitDto(
            Name: "Test Habit",
            Description: "Test Description",
            Type: HabitType.Binary,
            Frequency: new FrequencyDto(FrequencyType.Daily, 1),
            Targets: new TargetDto(10, "minutes"),
            EndDate: null,
            Milestone: null
        );

        var mockRepository = new Mock<IGenericRepository<Habit>>();
        mockRepository.Setup(r => r.Add(It.IsAny<Habit>()));

        _mockUnitOfWork.Setup(u => u.GetRepository<Habit>()).Returns(mockRepository.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _habitService.CreateAsync(createHabitDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Habit", result.Data.Name);
        Assert.StartsWith("h_", result.Data.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowEntityNotFoundException_WhenHabitDoesNotExist()
    {
        // Arrange
        var mockRepository = new Mock<IGenericRepository<Habit>>();
        mockRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Habit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Habit?)null);

        _mockUnitOfWork.Setup(u => u.GetRepository<Habit>()).Returns(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<DevHabit.Application.Exceptions.EntityNotFoundException>(
            () => _habitService.GetByIdAsync("nonexistent-id"));
    }
}
