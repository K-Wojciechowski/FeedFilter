import MenuItem from "@mui/material/MenuItem";
import Select from "@mui/material/Select";
import type { Decision } from "./models";

export interface DecisionPickerProps {
  value: Decision;
  id?: string;
  setValue: (value: Decision) => void;
  disabled?: boolean;
}

export default function DecisionPicker(props: DecisionPickerProps) {
  return (
    <Select
      fullWidth={true}
      id={props.id}
      value={props.value}
      onChange={(e) => props.setValue(e.target.value as Decision)}
      disabled={props.disabled}>
      <MenuItem value="accept">Accept</MenuItem>
      <MenuItem value="reject">Reject</MenuItem>
      <MenuItem value="promote">Promote</MenuItem>
      <MenuItem value="demote">Demote</MenuItem>
    </Select>
  );
}
