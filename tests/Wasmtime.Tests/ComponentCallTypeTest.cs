using Xunit;
using static Wit.Tests.Common.Types;

namespace Wasmtime.Tests;

public class ComponentCallTypeTest(ComponentFixture fixture)
{
    [Fact]
    public void SInt8()
    {
        using var state = fixture.CreateState();

        Assert.Equal((sbyte)42, state.Exports.AddS8(40, 2));
    }

    [Fact]
    public void UInt8()
    {
        using var state = fixture.CreateState();

        Assert.Equal((byte)42, state.Exports.AddU8(40, 2));
    }

    [Fact]
    public void SInt16()
    {
        using var state = fixture.CreateState();

        Assert.Equal((short)42, state.Exports.AddS16(40, 2));
    }

    [Fact]
    public void UInt16()
    {
        using var state = fixture.CreateState();

        Assert.Equal((ushort)42, state.Exports.AddU16(40, 2));
    }

    [Fact]
    public void SInt32()
    {
        using var state = fixture.CreateState();

        Assert.Equal(42, state.Exports.AddS32(40, 2));
    }

    [Fact]
    public void UInt32()
    {
        using var state = fixture.CreateState();

        Assert.Equal(42u, state.Exports.AddU32(40, 2));
    }

    [Fact]
    public void SInt64()
    {
        using var state = fixture.CreateState();

        Assert.Equal(42L, state.Exports.AddS64(40L, 2L));
    }

    [Fact]
    public void UInt64()
    {
        using var state = fixture.CreateState();

        Assert.Equal(42UL, state.Exports.AddU64(40UL, 2UL));
    }

    [Fact]
    public void Float32()
    {
        using var state = fixture.CreateState();

        Assert.Equal(4.2f, state.Exports.AddF32(4.0f, 0.2f), 3);
    }

    [Fact]
    public void Float64()
    {
        using var state = fixture.CreateState();

        Assert.Equal(4.2, state.Exports.AddF64(4.0, 0.2), 5);
    }

    [Fact]
    public void String()
    {
        using var state = fixture.CreateState();

        Assert.Equal("UPPERCASE", state.Exports.Uppercase("uppercase"));
    }

    [Fact]
    public void Void()
    {
        using var state = fixture.CreateState();

        Assert.False(state.Exports.GetFlag());
        state.Exports.SetFlag(true);
        Assert.True(state.Exports.GetFlag());
    }

    [Fact]
    public void Record()
    {
        using var state = fixture.CreateState();

        var result = state.Exports.AddPoint(
            new() { X = 1, Y = 3 },
            new() { X = 2, Y = 4 });

        Assert.Equal(3, result.X);
        Assert.Equal(7, result.Y);
    }

    [Fact]
    public void Record_Nested()
    {
        using var state = fixture.CreateState();

        state.Exports.RegisterEntity(new()
        {
            Id = 1,
            Name = "Entity",
            Position = new()
            {
                X = 10,
                Y = 20
            }
        });

        var entity = state.Exports.GetEntity(1);
        Assert.Equal(1, entity.Id);
        Assert.Equal("Entity", entity.Name);
        Assert.Equal(10, entity.Position.X);
        Assert.Equal(20, entity.Position.Y);
    }

    [Fact]
    public void List()
    {
        using var state = fixture.CreateState();

        var result = state.Exports.MultiplyList([1, 2, 3], 2);

        Assert.Equal([2, 4, 6], result);
    }

    [Fact]
    public void List_Nested()
    {
        using var state = fixture.CreateState();

        var result = state.Exports.SumNestedList([[1, 2], [3, 4]]);

        Assert.Equal(10u, result);
    }

    [Fact]
    public void List_Record()
    {
        using var state = fixture.CreateState();

        state.Exports.RegisterEntities([
            new()
            {
                Id = 1,
                Name = "A",
                Position = new() { X = 1, Y = 2 }
            },
            new()
            {
                Id = 2,
                Name = "B",
                Position = new() { X = 3, Y = 4 }
            }
        ]);

        var entities = state.Exports.GetEntities();

        Assert.Equal(2, entities.Length);

        Assert.Equal(1, entities[0].Id);
        Assert.Equal("A", entities[0].Name);
        Assert.Equal(1, entities[0].Position.X);
        Assert.Equal(2, entities[0].Position.Y);

        Assert.Equal(2, entities[1].Id);
        Assert.Equal("B", entities[1].Name);
        Assert.Equal(3, entities[1].Position.X);
        Assert.Equal(4, entities[1].Position.Y);
    }

    [Fact]
    public void List_Enum()
    {
        using var state = fixture.CreateState();

        var result = state.Exports.ReturnStatusList([Status.Active, Status.Inactive, Status.Pending]);

        Assert.Equal([Status.Active, Status.Inactive, Status.Pending], result);
    }

    [Fact]
    public void Enum()
    {
        using var state = fixture.CreateState();

        Assert.Equal(Status.Active, state.Exports.ReturnStatus(Status.Active));
    }
}