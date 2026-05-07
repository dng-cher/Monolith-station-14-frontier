using System;
using Content.Shared.Decals;
using NUnit.Framework;
using Robust.Shared.Maths;

namespace Content.Tests.Shared.Decals
{
    [TestFixture]
    public sealed class DecalSerializationTests
    {
        [Test]
        public void QuantizationRoundTripStaysWithinTolerance()
        {
            var values = new[] { 0f, 0.125f, 1.5f, 15.875f, 31.996f };

            foreach (var value in values)
            {
                var encoded = TestSharedDecalSystem.EncodeCoord(value);
                var decoded = TestSharedDecalSystem.DecodeCoord(encoded);
                Assert.That(MathF.Abs(decoded - value), Is.LessThanOrEqualTo(1f / TestSharedDecalSystem.Scale));
            }
        }

        [Test]
        public void NetDeltaPayloadStoresCompactDecalData()
        {
            var delta = new DecalChunkDelta
            {
                ResetChunk = true
            };

            delta.Upserts[1] = new NetDecalData
            {
                RelX = TestSharedDecalSystem.EncodeCoord(1.25f),
                RelY = TestSharedDecalSystem.EncodeCoord(2.5f),
                PrototypeNetId = 3,
                Color = Color.Red,
                Angle = Angle.FromDegrees(90),
                ZIndex = 4,
                Cleanable = true
            };
            delta.RemovedDecals.Add(77);

            Assert.That(delta.ResetChunk, Is.True);
            Assert.That(delta.Upserts[1].PrototypeNetId, Is.EqualTo((ushort) 3));
            Assert.That(TestSharedDecalSystem.DecodeCoord(delta.Upserts[1].RelX), Is.EqualTo(1.25f).Within(1f / TestSharedDecalSystem.Scale));
            Assert.That(delta.RemovedDecals, Contains.Item(77));
        }
    }

    internal sealed class TestSharedDecalSystem : SharedDecalSystem
    {
        public const float Scale = DecalCoordQuantScale;

        public static ushort EncodeCoord(float value) => QuantizeDecalCoord(value);
        public static float DecodeCoord(ushort value) => DequantizeDecalCoord(value);
    }
}
