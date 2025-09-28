import { type ReactElement } from "react";
import { useLocation } from "wouter";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";

export interface PageHeaderProps {
  title: string;
  showBackButton: boolean;
}

export default function PageHeader(props: PageHeaderProps): ReactElement {
  const [, navigate] = useLocation();

  return (
    <>
      <Typography variant="overline" sx={{ display: "block", fontWeight: "bold" }} color="primary">
        FeedFilter
      </Typography>
      <Typography
        variant="h2"
        gutterBottom
        sx={props.showBackButton ? { marginLeft: "-32px" } : {}}>
        {props.showBackButton && (
          <IconButton
            color="primary"
            aria-label="Go back"
            size="large"
            sx={{ width: "32px" }}
            onClick={() => navigate("/_admin")}>
            <ArrowBackIcon fontSize="inherit" />
          </IconButton>
        )}
        {props.title}
      </Typography>
    </>
  );
}
