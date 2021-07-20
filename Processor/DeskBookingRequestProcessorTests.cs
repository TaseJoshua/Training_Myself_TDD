using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;

        public DeskBookingRequestProcessorTests()
        {
            
            _request = new DeskBookingRequest
            {
                FirstName = "James",
                LastName = "Paul",
                Email = "thomasPaul@aol.com",
                Date = new DateTime(2021, 07, 25)
            };

            _availableDesks = new List<Desk> { new Desk {Id = 7 } };
            
            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();
            _deskRepositoryMock = new Mock<IDeskRepository>();
            _deskRepositoryMock.Setup(x => x.GetAvailableDesks(_request.Date))
                .Returns(_availableDesks);
            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object,_deskRepositoryMock.Object);
        }
        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            //Arrange
            

            //Act
            DeskBookingResult result =  _processor.BookDesk(_request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_request.FirstName, result.FirstName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestNull()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));
            //Assert
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            //Arrange
            DeskBooking saveDeskBooking = null;
            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                { saveDeskBooking = deskBooking; });

            //Act
            _processor.BookDesk(_request);
            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);
            //Assert

            Assert.NotNull(saveDeskBooking);
            Assert.Equal(_request.FirstName, saveDeskBooking.FirstName);
            Assert.Equal(_request.LastName, saveDeskBooking.LastName);
            Assert.Equal(_request.Email, saveDeskBooking.Email);
            Assert.Equal(_request.Date, saveDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, saveDeskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDeskIfNoDeskIsAvailable()
        {
            //Arrange
            _availableDesks.Clear();
            //Act
            _processor.BookDesk(_request);
            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);
        }

        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode (DeskBookingResultCode expectedResultCode, bool isDeskAvaialable)
        {
            //Arrange
            if (!isDeskAvaialable)
            {
                _availableDesks.Clear();
            }

            //Act
            var result = _processor.BookDesk(_request);

            //Assert
            Assert.Equal(expectedResultCode,result.Code);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpectedDeskBookingId(int? expectedDeskBookingId, bool isDeskAvaialable)
        {
            //Arrange
            if (!isDeskAvaialable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(deskBooking =>
                    {
                        deskBooking.Id = expectedDeskBookingId.Value;
                    });
            }

            //Act
            var result = _processor.BookDesk(_request);

            //Assert
            Assert.Equal(expectedDeskBookingId, result.DeskBookingId);
        }
    }
}
