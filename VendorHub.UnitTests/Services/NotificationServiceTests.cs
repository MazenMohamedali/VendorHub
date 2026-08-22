using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VendorHub.DTOs.NotificationDto;
using VendorHub.Events;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services;
using VendorHub.UnitTests.Extensions;

namespace VendorHub.UnitTests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<IGeneralRepository<Notification>> _notificationRepositoryMock;
        private readonly Mock<IEventQueue<OrderStatusChangedEvent>> _eventQueueMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<IGeneralRepository<Notification>>();
            _eventQueueMock = new Mock<IEventQueue<OrderStatusChangedEvent>>();
            _loggerMock = new Mock<ILogger<NotificationService>>();
        }

        private NotificationService CreateSut()
        {
            return new NotificationService(
                _notificationRepositoryMock.Object,
                _eventQueueMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllNotificationsPagedAsync_WhenInvoked_ReturnsPagedNotifications()
        {
            // Arrange
            const int userId = 10;
            var notifications = new List<Notification>
            {
                new() { Id = 1, UserId = userId, Type = NotificationType.NewPurchase, IsRead = false },
                new() { Id = 2, UserId = userId, Type = NotificationType.StatusUpdate, IsRead = true }
            };

            _notificationRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(notifications.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllNotificationsPagedAsync(userId, pageNumber: 1, pageSize: 10);

            // Assert
            var paged = result.ShouldBeSucceeded();
            paged.TotalCount.Should().Be(2);
            paged.Items.Should().NotBeNull();
            paged.Items!.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUnreadNotificationsAsync_WhenInvoked_ReturnsOnlyUnreadNotifications()
        {
            // Arrange
            const int userId = 10;
            var notifications = new List<Notification>
            {
                new() { Id = 1, UserId = userId, Type = NotificationType.NewPurchase, IsRead = false },
                new() { Id = 2, UserId = userId, Type = NotificationType.StatusUpdate, IsRead = true }
            };

            _notificationRepositoryMock
                .Setup(r => r.GetAllAsNoTracking())
                .Returns(notifications.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.GetUnreadNotificationsAsync(userId);

            // Assert
            var list = result.ShouldBeSucceeded().ToList();
            list.Should().HaveCount(1);
            list.First().Id.Should().Be(1);
            list.First().IsRead.Should().BeFalse();
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotificationNotFound_ReturnsNotFound()
        {
            // Arrange
            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notification?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAsReadAsync(99);

            // Assert
            result.ShouldBeNotFound();
            _notificationRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenAlreadyRead_ReturnsSucceededWithoutSaving()
        {
            // Arrange
            var notification = new Notification { Id = 1, IsRead = true };

            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAsReadAsync(1);

            // Assert
            result.ShouldBeSucceeded();
            _notificationRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenUnread_MarksAsReadAndSaves()
        {
            // Arrange
            var notification = new Notification { Id = 1, IsRead = false };

            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAsReadAsync(1);

            // Assert
            result.ShouldBeSucceeded();
            notification.IsRead.Should().BeTrue();
            _notificationRepositoryMock.Verify(r => r.Update(notification), Times.Once);
            _notificationRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenDbUpdateConcurrencyOccurs_ReturnsError()
        {
            // Arrange
            var notification = new Notification { Id = 1, IsRead = false };

            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            _notificationRepositoryMock
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAsReadAsync(1);

            // Assert
            result.ShouldBeError();
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WhenNoUnread_ReturnsSucceededWithoutSaving()
        {
            // Arrange
            _notificationRepositoryMock
                .Setup(r => r.GetAll())
                .Returns(new List<Notification>().BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAllAsReadAsync(userId: 5);

            // Assert
            result.ShouldBeSucceeded();
            _notificationRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WhenUnreadNotificationsExist_MarksAllAsReadAndSaves()
        {
            // Arrange
            const int userId = 5;
            var notif1 = new Notification { Id = 1, UserId = userId, IsRead = false };
            var notif2 = new Notification { Id = 2, UserId = userId, IsRead = false };

            _notificationRepositoryMock
                .Setup(r => r.GetAll())
                .Returns(new List<Notification> { notif1, notif2 }.BuildMock());

            var sut = CreateSut();

            // Act
            var result = await sut.MarkAllAsReadAsync(userId);

            // Assert
            result.ShouldBeSucceeded();
            notif1.IsRead.Should().BeTrue();
            notif2.IsRead.Should().BeTrue();
            _notificationRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notification?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteNotificationAsync(notificationId: 99, userId: 1);

            // Assert
            result.ShouldBeNotFound();
            _notificationRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenUserDoesNotOwnNotification_ReturnsForbidden()
        {
            // Arrange
            var notification = new Notification { Id = 10, UserId = 2 };

            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteNotificationAsync(notificationId: 10, userId: 99); // Different user

            // Assert
            result.ShouldBeForbidden();
            _notificationRepositoryMock.VerifyNoDatabaseMutations();
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenValid_DeletesNotificationAndSaves()
        {
            // Arrange
            var notification = new Notification { Id = 10, UserId = 2 };

            _notificationRepositoryMock
                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notification);

            var sut = CreateSut();

            // Act
            var result = await sut.DeleteNotificationAsync(notificationId: 10, userId: 2);

            // Assert
            result.ShouldBeSucceeded();
            _notificationRepositoryMock.Verify(r => r.Delete(notification), Times.Once);
            _notificationRepositoryMock.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
