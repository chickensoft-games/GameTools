namespace Chickensoft.GameTools.Tests.Displays;

using Chickensoft.GameTools.Displays;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class WindowScaleInfoTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public void ToStringDescription()
  {
    var scaleInfo = new WindowScaleInfo(
      Screen: 1,
      SystemScale: 2.0f,
      WindowScale: 3.0f,
      DisplayScale: 4.0f,
      ThemeScale: 5.0f,
      ContentScaleFactor: 6.0f,
      CorrectionFactor: 7.0f,
      ProjectViewportSize: new Vector2I(8, 9),
      ProjectWindowSize: new Vector2I(10, 11),
      NativeResolution: new Vector2I(12, 13),
      LogicalResolution: new Vector2I(14, 15),
      ScreenPosition: new Vector2I(16, 17)
    );

    scaleInfo.ToString().ShouldBe("""
    Screen: 1
    SystemScale: 2
    WindowScale: 3
    DisplayScale: 4
    ThemeScale: 5
    ContentScaleFactor: 6
    CorrectionFactor: 7
    ProjectViewportSize: (8, 9)
    ProjectWindowSize: (10, 11)
    NativeResolution: (12, 13)
    LogicalResolution: (14, 15)
    ScreenPosition: (16, 17)
    """, StringCompareShould.IgnoreLineEndings);
  }
}
