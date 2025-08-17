import { type ReactElement } from "react";
import { TextField } from "@mui/material";
import IconButton from "@mui/material/IconButton";
import Box from "@mui/material/Box";
import ArrowCircleRightOutlinedIcon from "@mui/icons-material/ArrowCircleRightOutlined";

export default function PasswordPrompt(): ReactElement {
  return (
    <Box
      sx={{
        width: "100vw",
        height: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}>
      <TextField type="password" />
      <IconButton color="primary" aria-label="Go" size="large" sx={{ ml: 1 }}>
        <ArrowCircleRightOutlinedIcon fontSize="inherit" />
      </IconButton>
    </Box>
  );
}
