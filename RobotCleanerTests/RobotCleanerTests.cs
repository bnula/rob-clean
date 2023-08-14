using Xunit;
using Newtonsoft.Json;
using RobotCleaner.RobotCleaner;
using System;

namespace RobotCleanerTests
{
    public class RobotTests
    {
        // TODO: Add missing unit tests
        [Fact(DisplayName = "Robot Successfully Finishes Instructions and Returns Cleaned Cells list")]
        public void GetsCorrectInputData_Returns_CleanedCells()
        {
            var successfulInput = @"
    {
        ""map"": [  [""S"", ""S"", ""S"", ""S""],
                    [""S"", ""S"", ""S"", ""S""],
                    [""S"", ""S"", ""S"", ""S""]],
        ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""N""},
        ""commands"": [""C"", ""TR"", ""A"", ""C"", ""TR"", ""A"", ""C"", ""TR"", ""A"", ""C""],
        ""battery"": 100
    }";

            var inputData = JsonConvert.DeserializeObject<InputData>(successfulInput);
            var robot = new Robot(inputData);
            var outputData = robot.ProcessCommands();

            // Assuming the robot ends at position (1, 1) facing north and cleaned every position it visited.
            Assert.Equal(0, robot.CurrentPosition.X);
            Assert.Equal(1, robot.CurrentPosition.Y);
            Assert.Equal("W", robot.CurrentPosition.Facing);
            Assert.Contains(new ResultPosition { X = 0, Y = 0 }, outputData.Cleaned);
            Assert.Contains(new ResultPosition { X = 0, Y = 1 }, outputData.Cleaned);
            Assert.Contains(new ResultPosition { X = 1, Y = 0 }, outputData.Cleaned);
            Assert.Contains(new ResultPosition { X = 1, Y = 1 }, outputData.Cleaned);
            Assert.True(robot.Battery > 0);
        }

        [Fact(DisplayName = "Getting Invalid Input Data throws an Error")]
        public void GetsInvalidInput_Throws_ArfumentException()
        {
            var invalidInput = @"
            {
                ""map"": [],
                ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""N""},
                ""commands"": [""TL""],
                ""battery"": 100
            }";

            var inputData = JsonConvert.DeserializeObject<InputData>(invalidInput);
            Assert.Throws<ArgumentException>(() => new Robot(inputData));
        }

        [Fact(DisplayName = "No Path From Start")]
        public void HasNoPathFromStart_Returns_StartingPosition()
        {
            var noPathInput = @"
        {
            ""map"": [[""C"", ""C"", ""C"", ""C""]],
            ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""N""},
            ""commands"": [""TL"", ""A"", ""C""],
            ""battery"": 100
        }";

            var inputData = JsonConvert.DeserializeObject<InputData>(noPathInput);
            var robot = new Robot(inputData);
            robot.ProcessCommands();

            Assert.Equal(0, robot.CurrentPosition.X);
            Assert.Equal(0, robot.CurrentPosition.Y);
        }

        [Fact(DisplayName = "Out Of Battery returns lists when finished mid-instructions")]
        public void IsOutOfBattery_Returns_DataUntilOutOfBattery()
        {
            var outOfBatteryInput = @"
        {
            ""map"": [[""S"", ""S"", ""S"", ""S""]],
            ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""N""},
            ""commands"": [""TR"", ""A"", ""C"", ""TL"", ""A"", ""C"", ""TL"", ""A"", ""C"", ""TL"", ""A"", ""C""],
            ""battery"": 3
        }";

            var inputData = JsonConvert.DeserializeObject<InputData>(outOfBatteryInput);
            var robot = new Robot(inputData);
            var result = robot.ProcessCommands();

            Assert.True(robot.Battery <= 0);
            Assert.True(result.Visited.Count == 2);
            Assert.True(result.Cleaned.Count == 0);
        }

        [Fact(DisplayName = "Back Off Sequence Triggered when an obstacle is hit")]
        public void ObstacleIsHit_BackOffSequenceTriggered()
        {
            var backOffInput = @"
        {
            ""map"": [[""S"", ""S"", ""C"", ""S""]],
            ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""N""},
            ""commands"": [""TL"", ""A""],
            ""battery"": 100
        }";

            var inputData = JsonConvert.DeserializeObject<InputData>(backOffInput);
            var robot = new Robot(inputData);
            robot.ProcessCommands();

            // Assuming the backoff sequence puts the robot to (2, 0)
            Assert.Equal(1, robot.CurrentPosition.X);
            Assert.Equal(0, robot.CurrentPosition.Y);
        }

        [Fact(DisplayName = "Robot is Stuck")]
        public void Test_RobotStuck()
        {
            var robotStuckInput = @"
        {
            ""map"": [[""S"", ""C"", ""C"", ""S""], [""C"", ""C"", ""C"", ""C""]],
            ""start"": {""X"": 0, ""Y"": 0, ""facing"": ""E""},
            ""commands"": [""A"", ""TR"", ""A"", ""A"", ""C""],
            ""battery"": 100
        }";

            var inputData = JsonConvert.DeserializeObject<InputData>(robotStuckInput);
            var robot = new Robot(inputData);
            var result = robot.ProcessCommands();

            // Assuming the robot gets stuck at (1, 0)
            Assert.Equal(0, robot.CurrentPosition.X);
            Assert.Equal(0, robot.CurrentPosition.Y);
            Assert.True(result.Cleaned.Count == 0);
            Assert.True(result.Visited.Count == 1);
        }
    }
}