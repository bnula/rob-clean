using RobotCleaner.Exceptions;

namespace RobotCleaner.RobotCleaner
{
    public class Robot
    {
        private string[][] _map;
        public int Battery;
        public Position CurrentPosition;
        private bool _logsEnabled = false;

        private List<ResultPosition> _visited { get; set; }
        private List<ResultPosition> _cleaned { get; set;  }
        private List<string> _commands { get; set; }
        private List<string>? _logs { get; set; }

        public Robot(InputData inputData)
        {
            // add exception for empty fields
            if (inputData.Map.Length == 0 ||
                inputData == null ||
                inputData.Commands.Count() == 0 ||
                inputData.Battery == 0 ||
                inputData.Start == null)
            {
                throw new ArgumentException("Invalid input data");
            }
            // TODO: add validation for input fields
            if (inputData.Start.X < 0 ||
                inputData.Start.X > inputData.Map.Length ||
                inputData.Start.Y < 0 ||
                inputData.Start.Y > inputData.Map.Rank
                )
            {
                throw new ArgumentException();
            }

            _map = inputData.Map;
            Battery = inputData.Battery;
            CurrentPosition = inputData.Start;
            _commands = inputData.Commands;
            _visited = new List<ResultPosition>
            {
                new ResultPosition { X = CurrentPosition.X, Y = CurrentPosition.Y }
            };
            _cleaned = new List<ResultPosition>();
        }

        public List<string> GetLogs()
        {
            return _logs;
        }

        public OutputData ProcessCommands(bool withLogs = false)
        {
            if (withLogs) {
                _logsEnabled = true;
                _logs = new List<string>();
            }
            foreach (var command in _commands)
            {
                if (_logsEnabled) _logs.Add($"Command: {command}");
                switch (command.ToUpper())
                {
                    case "TL":
                        Turn(-90);
                        Battery--;
                        break;
                    case "TR":
                        Turn(90);
                        Battery--;
                        break;
                    case "A":
                        if (!Advance()) HandleObstacle();
                        Battery -= 2;
                        break;
                    case "B":
                        Back();
                        Battery -= 3;
                        break;
                    case "C":
                        Clean();
                        Battery -= 5;
                        break;
                    default:
                        throw new InvalidCommandException($"Invalid command {command}.");
                }

                if (Battery <= 0) break;
            }
            return GetOutputData();
        }

        private OutputData GetOutputData()
        {
            var cleanedSet = _cleaned.OrderBy(i => i.X).ThenBy(i => i.Y).ToHashSet();
            var visitedSet = _visited.OrderBy(i => i.X).ThenBy(i => i.Y).ToHashSet();
            var result = new OutputData
            {
                Battery = Battery,
                Cleaned = cleanedSet,
                Final = CurrentPosition,
                Visited = visitedSet
            };
            return result;
        }

        private void Turn(int angle)
        {
            var directions = new List<string> { "N", "E", "S", "W" };
            int currentIndex = directions.IndexOf(CurrentPosition.Facing);
            if (currentIndex == -1)
            {
                throw new InvalidDirectionException($"Invalid direction value - {CurrentPosition.Facing}.");
            };
            int newIndex = (currentIndex + angle / 90 + 4) % 4;

            CurrentPosition.Facing = directions[newIndex];
        }

        private bool Advance()
        {
            var newPosition = GetNewPosition();

            if (IsValidMove(newPosition))
            {
                CurrentPosition = newPosition;
                if (!_visited.Contains(new ResultPosition { X = CurrentPosition.X, Y = CurrentPosition.Y }))
                {
                    _visited.Add(new ResultPosition { X = CurrentPosition.X, Y = CurrentPosition.Y });
                }
                return true;
            }

            return false;
        }

        private void Back()
        {
            Turn(180);
            Advance();
            Turn(180);
        }

        private void Clean()
        {
            if (_map[CurrentPosition.Y][CurrentPosition.X] == "S")
            {
                _cleaned.Add(new ResultPosition { X = CurrentPosition.X, Y = CurrentPosition.Y });
                // mark the position as cleaned
                _map[CurrentPosition.Y][CurrentPosition.X] = "D";
            }
        }

        private Position GetNewPosition()
        {
            Position newPosition = new Position { X = CurrentPosition.X, Y = CurrentPosition.Y, Facing = CurrentPosition.Facing };

            switch (CurrentPosition.Facing.ToUpper())
            {
                case "N":
                    newPosition.Y--;
                    break;
                case "E":
                    newPosition.X++;
                    break;
                case "S":
                    newPosition.Y++;
                    break;
                case "W":
                    newPosition.X--;
                    break;
                default:
                    throw new InvalidDirectionException($"Invalid direction value - {CurrentPosition.Facing}");
            }

            return newPosition;
        }

        private bool IsValidMove(Position position)
        {
            if (position.Y < 0 || position.Y >= _map.Length ||
                position.X < 0 || position.X >= _map[0].Length)
            {
                return false;
            }

            var cell = _map[position.Y][position.X];
            if (cell == "D" || cell == "S")
            {
                return true;
            }

            return false;
        }

        private void HandleObstacle()
        {
            List<List<string>> backOffStrategies = new List<List<string>>
            {
                new List<string> { "TR", "A", "TL" },
                new List<string> { "TR", "A", "TR" },
                new List<string> { "TR", "A", "TR" },
                new List<string> { "TR", "B", "TR", "A" },
                new List<string> { "TL", "TL", "A" }
            };

            foreach (var strategy in backOffStrategies)
            {
                bool success = true;

                foreach (var command in strategy)
                {
                    if (_logsEnabled) _logs.Add($"BackOffStrat: Step: {command}");
                    switch (command)
                    {
                        case "TL":
                            Turn(-90);
                            Battery--;
                            break;
                        case "TR":
                            Turn(90);
                            Battery--;
                            break;
                        case "A":
                            success = Advance();
                            Battery -= 2;
                            break;
                        case "B":
                            Back();
                            Battery -= 3;
                            break;
                        default:
                            throw new InvalidCommandException($"invalid command {command}");
                    }

                    if (Battery <= 0 || !success) break;
                }

                if (success) return;
            }
        }
    }
}
