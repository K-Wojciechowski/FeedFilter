import { type ReactElement } from "react";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
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
      contrastText: "#000"
    },
    secondary: {
      light: "#bae2ff",
      main: "#0066ff",
      dark: "#2341e0",
      contrastText: "#fff"
    }
  },
  typography: {
    fontFamily: "system-ui, sans-serif"
  }
});

export default function App(): ReactElement {
  const page = useFeedFilterStore((store) => store.page);
  const goToPage = useFeedFilterStore((store) => store.goToPage);
  const editedFeed = useFeedFilterStore((store) => store.editedFeed);

  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      {page === "auth" && <PasswordPrompt />}
      {page !== "auth" && <>
        <Container
          maxWidth="lg"
          component="main">
          {page === "list" && <ListPage />}
          {page === "create" && <EditPage mode="create" />}
          {page === "edit" && <EditPage mode="edit" editedFeed={editedFeed} />}
          <Zoom key={page} in={page === "list"}>
            <Fab color="primary" aria-label="add" onClick={() => goToPage("create")}
                 sx={{ position: "absolute", bottom: 16, right: 16 }}>
              <AddIcon />
            </Fab>
          </Zoom>
        </Container>
      </>
      }
    </ThemeProvider>
  );
}
