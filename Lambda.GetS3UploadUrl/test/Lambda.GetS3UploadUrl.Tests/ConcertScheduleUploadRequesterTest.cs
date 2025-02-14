using Amazon.S3;
using Amazon.S3.Model;
using Xunit;
using Lambda.GetS3UploadUrl.UploadRequester;
using LPCalendar.DataStructure.Requests;
using Moq;

namespace Lambda.GetS3UploadUrl.Tests;

public class ConcertScheduleUploadRequesterTest
{
    private readonly Mock<IAmazonS3> _s3Mock = new();
    private readonly Mock<IGuidService> _guidServiceMock = new();
    
    [Theory]
    [InlineData("image/png", "test-id", GetS3UploadUrlRequest.FileType.ConcertSchedule, "https://test.loc/test-bucket-1234567/test-id/schedule/")]
    //[InlineData("image/jpeg", "other-id", GetS3UploadUrlRequest.FileType.LpuInfo, "https://test.loc/bucket/12345")]
    public void GetUrl(string contentType, string concertId, GetS3UploadUrlRequest.FileType fileType, string expectedUrl)
    {
        var testGuid = Guid.NewGuid();
        
        // Setup mocks
        _guidServiceMock.Setup(gs => gs.Random())
            .Returns(testGuid);
        
        int getPresignedUrlCalled = 0;
        _s3Mock.Setup(m => m.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(urlRequest =>
            {
                getPresignedUrlCalled++;
                
                Assert.Equal(contentType, urlRequest.ContentType);
                Assert.Equal("test-bucket-1234567", urlRequest.BucketName);
            })
            .Returns<GetPreSignedUrlRequest>(urlRequest 
                => $"https://test.loc/{urlRequest.BucketName}/{urlRequest.Key}");
        
        // generate upload request
        var uploadRequest = new GetS3UploadUrlRequest
        {
            ConcertId = concertId,
            ContentType = contentType,
            Type = fileType
        };
        
        // run the method
        var requester = new ConcertScheduleUploadRequester(uploadRequest, _guidServiceMock.Object, _s3Mock.Object);
        var generatedUrl = requester.GetUploadUrl();
        
        // verify result
        Assert.Equal(expectedUrl + testGuid, generatedUrl);
        
        Assert.Equal(1, getPresignedUrlCalled);
    }
}
