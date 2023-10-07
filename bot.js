function createBoard(rows, columns, starTiles) {
  // Create an empty board
  const board = [];

  // Loop to create X rows
  let index = 0;

  for (let y = 0; y < rows; y++) {
    const row = [];

    // Loop to create Y columns for each row
    for (let x = 0; x < columns; x++) {
      let value = "";

      // Check if the current (i, j) id is in the "values" array
      for (const item of starTiles) {
        if (item.id === `${y}-${x}`) {
          value = item.value; // Override with the value from "values"
          break; // No need to keep checking
        }
      }

      const IsOnBorder = (direction) => {
        let isOnBorder = false;

        if (direction == "Up" && y == 0) isOnBorder = true;
        if (direction == "Down" && y == rows - 1) isOnBorder = true;
        if (direction == "Left" && x == 0) isOnBorder = true;
        if (direction == "Right" && x == columns - 1) isOnBorder = true;

        return isOnBorder;
      };

      const GetAdjacentTile = (tiles, direction) => {
        if (IsOnBorder(direction)) return null;

        try {
          if (direction == "Up") return tiles.at(index - columns);
          if (direction == "Down") return tiles.at(index + columns);
          if (direction == "Left") return tiles.at(index - 1);
          if (direction == "Right") return tiles.at(index + 1);
        } catch (Exception) {
          return null;
        }

        return null;
      };

      const GetAdjacentTileWithStarInDirection = (tiles, direction) => {
        var adjacentTile = GetAdjacentTile(tiles, direction);

        if (adjacentTile == null) return null;
        if (!adjacentTile.HasStar())
          return adjacentTile.GetAdjacentTileWithStarInDirection(direction);

        return adjacentTile;
      };

      // Push the cell value to the row
      const tile = {
        value,
        IsOnBorder,
        GetAdjacentTile,
        GetAdjacentTileWithStarInDirection,
        HasStar() {
          return this.value == "ON";
        },
        Toggle() {
          if (this.value == "ON") {
            this.value = "OFF";
          } else {
            this.value = "ON";
          }
        },
      };

      row.push(tile);

      index++;
    }

    // Push the row to the board
    board.push(row);
  }

  return board;
}

// Example usage:
const rows = 6;
const columns = 3;
const starTiles = [
  { id: "0-1", value: "ON" },
  { id: "0-2", value: "OFF" },
  { id: "2-0", value: "ON" },
  { id: "2-1", value: "OFF" },
  { id: "5-0", value: "OFF" },
  { id: "5-1", value: "OFF" },
  { id: "5-2", value: "OFF" },
];
const tiles = createBoard(rows, columns, starTiles);

function resolveBoard() {
  const startingTile = tiles[2][0];

  console.log(startingTile.value);
  console.log(startingTile.Toggle());
  console.log(startingTile.value);

  while (starTiles.filter(({ v }) => v === "ON").length != values.length) {}
}

function rand(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

resolveBoard();
