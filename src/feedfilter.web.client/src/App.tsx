import { type ReactElement } from "react";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import { Router, Route, useLocation } from "wouter";
import PasswordPrompt from "./PasswordPrompt.tsx";
import Container from "@mui/material/Container";
import Fab from "@mui/material/Fab";
import Zoom from "@mui/material/Zoom";
import AddIcon from "@mui/icons-material/Add";
import EditPage from "./EditPage.tsx";
import ListPage from "./ListPage.tsx";
import useFeedFilterStore from "./state.ts";

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
  const [location, navigate] = useLocation();
  const token = useFeedFilterStore((store) => store.token);
  const isAuthenticated = token !== undefined;

  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <Router>
        {!isAuthenticated ? (
          <PasswordPrompt />
        ) : (
          <Container maxWidth="lg" component="main">
            <Route path="/_admin" component={ListPage} />
            <Route path="/_create" component={() => <EditPage />} />
            <Route path="/_edit/:feedId" component={EditPage} />
            <Zoom key={location} in={location === "/_admin"}>
              <Fab
                color="primary"
                aria-label="add"
                onClick={() => navigate("/_create")}
                sx={{ position: "absolute", bottom: 16, right: 16 }}>
                <AddIcon />
              </Fab>
            </Zoom>
          </Container>
        )}
      </Router>
    </ThemeProvider>
  );
}
