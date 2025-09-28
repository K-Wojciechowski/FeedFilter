import { useCallback, useState, type ReactElement } from "react";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import TextField from "@mui/material/TextField";
import ArrowCircleRightOutlinedIcon from "@mui/icons-material/ArrowCircleRightOutlined";
import useFeedFilterStore from "./state.ts";

export default function PasswordPrompt(): ReactElement {
  const configuredToken = useFeedFilterStore((state) => state.token);
  const loading = useFeedFilterStore((state) => state.loading);

  const goToPage = useFeedFilterStore((state) => state.goToPage);
  const init = useFeedFilterStore((state) => state.init);

  const [tokenInput, setTokenInput] = useState("");
  const [error, setError] = useState(false);

  const doSubmit = useCallback(async () => {
    try {
      setError(false);
      await init(tokenInput);
    } catch (error) {
      setError(true);
    }
  }, [init, tokenInput]);

  if (configuredToken != undefined) {
    goToPage("list");
    return <></>;
  }

  return (
    <Box
      sx={{
        width: "100vw",
        height: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center"
      }}>
      <form onSubmit={async (event) => {
        event.preventDefault();
        await doSubmit();
      }}>
        <TextField
          type="password"
          autoFocus
          error={error}
          value={tokenInput}
          disabled={loading}
          onChange={e => setTokenInput(e.target.value)}
          />
        <IconButton color="primary" aria-label="Go" size="large" sx={{ ml: 1 }} type="submit" loading={loading}>
          <ArrowCircleRightOutlinedIcon fontSize="inherit" />
        </IconButton>
      </form>
    </Box>
  );
}
