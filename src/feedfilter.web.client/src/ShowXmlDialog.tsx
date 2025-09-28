import * as React from 'react';
import Dialog from '@mui/material/Dialog';
import Grid from '@mui/material/Grid';
import OutlinedInput from '@mui/material/OutlinedInput';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import CloseIcon from '@mui/icons-material/Close';
import Slide from '@mui/material/Slide';
import { type TransitionProps } from '@mui/material/transitions';

const Transition = React.forwardRef(function Transition(
  props: TransitionProps & {
    children: React.ReactElement<unknown>;
  },
  ref: React.Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

export interface ShowXmlDialogProps {
  feedId: string;
  originalXml: string;
  filteredXml: string;
  open: boolean;
  setOpen: (open: boolean) => void;
}

export default function ShowXmlDialog(props: ShowXmlDialogProps) {
  const handleClose = () => props.setOpen(false);
  return (
      <Dialog
        fullScreen
        open={props.open}
        onClose={() => handleClose}
        slots={{
          transition: Transition,
        }}
      >
        <AppBar sx={{ position: 'relative' }}>
          <Toolbar>
            <IconButton
              edge="start"
              color="inherit"
              onClick={handleClose}
              aria-label="close"
            >
              <CloseIcon />
            </IconButton>
            <Typography sx={{ ml: 2, flex: 1 }} variant="h6" component="div">
              XML for feed '{props.feedId}'
            </Typography>
          </Toolbar>
        </AppBar>
        <Grid container spacing={2} sx={{m: 2}}>
          <Grid size={6}>
            <Typography variant="h6">Original XML</Typography>
            <OutlinedInput readOnly multiline fullWidth value={props.originalXml} sx={{fontFamily: "Monospace"}} />
          </Grid>
          <Grid size={6}>
            <Typography variant="h6">Filtered XML</Typography>
            <OutlinedInput readOnly multiline fullWidth value={props.filteredXml} sx={{fontFamily: "Monospace"}} />
          </Grid>
        </Grid>
      </Dialog>
  );
}