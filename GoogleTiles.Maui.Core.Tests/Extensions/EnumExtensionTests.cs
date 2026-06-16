using GoogleTiles.Maui.Core.Extensions;

namespace GoogleTiles.Maui.Core.Tests.Extensions;

[TestFixture]
public class EnumExtensionTests
{
    [TestFixture]
    public class MapTypeTests
    {
        [TestCase(MapType.Roadmap, "roadmap")]
        [TestCase(MapType.Satellite, "satellite")]
        [TestCase(MapType.Terrain, "terrain")]
        [TestCase(MapType.Streetview, "streetview")]
        public void ToApiString_GivenValid_ShouldConvert(MapType type, string expected)
        {
            var result = type.ToApiString();
            result.Should().Be(expected);
        }

        [Test]
        public void ToApiString_GivenInvalid_ShouldThrow()
        {
            var act = () =>
            {
                const MapType mapType = (MapType)4;
                mapType.ToApiString();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    [TestFixture]
    public class ImageFormatTests
    {
        [TestCase(ImageFormat.Png, "png")]
        [TestCase(ImageFormat.Jpeg, "jpeg")]
        public void ToApiString_GivenValid_ShouldConvert(ImageFormat format, string expected)
        {
            var result = format.ToApiString();
            result.Should().Be(expected);
        }

        [Test]
        public void ToApiString_GivenInvalid_ShouldThrow()
        {
            var act = () =>
            {
                const ImageFormat format = (ImageFormat)2;
                format.ToApiString();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    [TestFixture]
    public class ScaleFactorTests
    {
        [TestCase(ScaleFactor.ScaleFactor1x, "scaleFactor1x")]
        [TestCase(ScaleFactor.ScaleFactor2x, "scaleFactor2x")]
        [TestCase(ScaleFactor.ScaleFactor4x, "scaleFactor4x")]
        public void ToApiString_GivenValid_ShouldConvert(ScaleFactor scale, string expected)
        {
            var result = scale.ToApiString();
            result.Should().Be(expected);
        }

        [Test]
        public void ToApiString_GivenInvalid_ShouldThrow()
        {
            var act = () =>
            {
                const ScaleFactor factor = (ScaleFactor)3;
                factor.ToApiString();
            };
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}