import { ThemeProvider, createTheme } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import { type ReactElement } from "react";
import PasswordPrompt from "./PasswordPrompt.tsx";

const appTheme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      light: "#ffe0b2",
      main: "#ff9900",
      dark: "#e65200",
      contrastText: "#000",
    },
    secondary: {
      light: "#bae2ff",
      main: "#0066ff",
      dark: "#2341e0",
      contrastText: "#fff",
    },
  },
  typography: {
    fontFamily: "system-ui, sans-serif",
  },
});

export default function App(): ReactElement {
  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <PasswordPrompt />
    </ThemeProvider>
  );
}
