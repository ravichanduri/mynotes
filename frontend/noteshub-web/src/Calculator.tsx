import { useState } from "react";
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  TextField,
  Typography,
} from "@mui/material";

export default function Calculator() {
  const [display, setDisplay] = useState("");

  const append = (value: string) => {
    setDisplay((prev) => prev + value);
  };

  const clear = () => {
    setDisplay("");
  };

  const removeLast = () => {
    setDisplay((prev) => prev.slice(0, -1));
  };

  const calculate = () => {
    try {
      // Only for demo/testing UI
      // eslint-disable-next-line no-eval
      const result = eval(display);
      setDisplay(String(result));
    } catch {
      setDisplay("Error");
    }
  };

  const buttons = [
    "7",
    "8",
    "9",
    "/",
    "4",
    "5",
    "6",
    "*",
    "1",
    "2",
    "3",
    "-",
    "0",
    ".",
    "=",
    "+",
  ];

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      bgcolor="#f4f4f4"
    >
      <Card sx={{ width: 360, borderRadius: 3, boxShadow: 5 }}>
        <CardContent>
          <Typography
            variant="h4"
            align="center"
            gutterBottom
            fontWeight="bold"
          >
            Calculator
          </Typography>

          <TextField
            fullWidth
            value={display}
            variant="outlined"
            InputProps={{
              readOnly: true,
            }}
            sx={{
              mb: 2,
              "& input": {
                textAlign: "right",
                fontSize: "2rem",
                fontWeight: "bold",
              },
            }}
          />

          <Grid container spacing={1}>
            <Grid size={6}>
              <Button
                fullWidth
                variant="contained"
                color="error"
                onClick={clear}
              >
                AC
              </Button>
            </Grid>

            <Grid size={6}>
              <Button
                fullWidth
                variant="contained"
                color="warning"
                onClick={removeLast}
              >
                ⌫
              </Button>
            </Grid>

            {buttons.map((btn) => (
              <Grid size={3} key={btn}>
                <Button
                  fullWidth
                  variant={btn === "=" ? "contained" : "outlined"}
                  color={btn === "=" ? "success" : "primary"}
                  sx={{
                    height: 60,
                    fontSize: "1.3rem",
                    fontWeight: "bold",
                  }}
                  onClick={() => {
                    if (btn === "=") {
                      calculate();
                    } else {
                      append(btn);
                    }
                  }}
                >
                  {btn}
                </Button>
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>
    </Box>
  );
}