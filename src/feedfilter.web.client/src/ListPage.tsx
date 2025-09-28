import { useCallback, useState, type ReactElement } from "react";
import PageHeader from "./PageHeader.tsx";
import Alert from "@mui/material/Alert";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Tooltip from "@mui/material/Tooltip";
import Paper from "@mui/material/Paper";
import IconButton from "@mui/material/IconButton";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import useFeedFilterStore from "./state.ts";
import { copyLink } from "./links.ts";

const actionCellWidth = "116px";

interface DeleteState {
  visible: boolean;
  feedId: string;
  loading: boolean;
  error: string | undefined;
}

export default function ListPage(): ReactElement {
  const feeds = useFeedFilterStore((state) => state.feeds);
  const startEditing = useFeedFilterStore((state) => state.startEditing);
  const callDeleteApi = useFeedFilterStore((state) => state.delete);
  const [copiedFeed, setCopiedFeed] = useState<string | undefined>(undefined);
  const [deleteState, setDeleteState] = useState<DeleteState>({
    visible: false,
    feedId: "",
    loading: false,
    error: undefined
  });

  const copyFeed = useCallback(async (feedId: string) => {
    await copyLink(feedId);
    setCopiedFeed(feedId);
    setTimeout(() => setCopiedFeed(f => f === feedId ? undefined : f), 1000);
  }, []);

  const openDeleteDialog = useCallback((feedId: string) => {
    setDeleteState({ visible: true, feedId, loading: false, error: undefined });
  }, []);

  const closeDeleteDialog = useCallback(() => {
    setDeleteState(s => ({ ...s, visible: false }));
  }, []);

  const doDelete = useCallback(async (feedId: string | undefined) => {
    if (feedId == undefined) return;

    setDeleteState(s => ({ ...s, loading: true, error: undefined }));

    try {
      await callDeleteApi(feedId);
      setDeleteState(s => ({ ...s, visible: false, error: undefined }));
    } catch (e: any) {
      setDeleteState(s => ({ ...s, loading: false, error: e.toString() }));
    }
  }, [callDeleteApi]);

  return <>
    <PageHeader title={"Feeds"} showBackButton={false} />
    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} aria-label="feed table">
        <TableHead>
          <TableRow sx={{ th: { fontWeight: 600 } }}>
            <TableCell>ID</TableCell>
            <TableCell>Description</TableCell>
            <TableCell sx={{ width: actionCellWidth }}>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {feeds.map((feed) => (
            <TableRow
              key={feed.feedId}
              sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
            >
              <TableCell component="th" scope="row" sx={{ fontFamily: 'Monospace' }}>
                {feed.feedId}
              </TableCell>
              <TableCell>{feed.description}</TableCell>
              <TableCell sx={{ width: actionCellWidth }} align="right">
                <Tooltip title={copiedFeed === feed.feedId ? "Copied!" : "Copy Link to Clipboard"}>
                  <IconButton aria-label="copy" size="small"
                              color={copiedFeed === feed.feedId ? "success" : undefined}
                              disableRipple={copiedFeed === feed.feedId}
                              onClick={() => copiedFeed === feed.feedId ? undefined : copyFeed(feed.feedId)}>
                    <ContentCopyIcon fontSize="inherit" />
                  </IconButton>
                </Tooltip>

                <Tooltip title="Edit">
                  <IconButton aria-label="edit" size="small" onClick={() => startEditing(feed)}>
                    <EditIcon fontSize="inherit" />
                  </IconButton>
                </Tooltip>

                <Tooltip title="Delete">
                  <IconButton aria-label="delete" size="small" onClick={() => openDeleteDialog(feed.feedId)}>
                    <DeleteIcon fontSize="inherit" />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
    <Dialog
      open={deleteState.visible}
      onClose={closeDeleteDialog}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">Delete “{deleteState.feedId}”?</DialogTitle>
      <DialogContent>
        <DialogContentText id="alert-dialog-description">
          {deleteState.error != undefined && <Alert severity="error">{deleteState.error}</Alert>}
          This action cannot be undone.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => doDelete(deleteState.feedId)} loading={deleteState.loading}>Yes</Button>
        <Button onClick={closeDeleteDialog} autoFocus disabled={deleteState.loading}>No</Button>
      </DialogActions>
    </Dialog>
  </>;
}
