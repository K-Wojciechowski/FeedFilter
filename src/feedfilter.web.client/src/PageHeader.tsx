import { type ReactElement } from "react";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import useFeedFilterStore from "./state.ts";

export interface PageHeaderProps {
  title: string;
  showBackButton: boolean;
}

export default function PageHeader(props: PageHeaderProps): ReactElement {
  const goToPage = useFeedFilterStore((state) => state.goToPage);

  return <>
    <Typography variant="overline" sx={{display: "block", fontWeight: "bold"}} color="primary">
      FeedFilter
    </Typography>
    <Typography variant="h2" gutterBottom sx={props.showBackButton ? { marginLeft: "-32px" } : {}}>
      {props.showBackButton &&
        <IconButton color="primary" aria-label="Go" size="large" sx={{ width: "32px" }} onClick={() => goToPage("list")}>
          <ArrowBackIcon fontSize="inherit" />
        </IconButton>}
      {props.title}
    </Typography>
  </>;
}