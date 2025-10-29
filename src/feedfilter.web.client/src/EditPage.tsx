import { useCallback, useState, type ReactElement } from "react";
import { useLocation } from "wouter";
import PageHeader from "./PageHeader.tsx";
import type { Decision, FeedFilteringResult, FeedUpdate, Rule } from "./models.ts";
import Alert from "@mui/material/Alert";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import Grid from "@mui/material/Grid";
import FormControl from "@mui/material/FormControl";
import OutlinedInput from "@mui/material/OutlinedInput";
import TextField from "@mui/material/TextField";
import FormLabel from "@mui/material/FormLabel";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import DeleteIcon from "@mui/icons-material/Delete";
import SaveIcon from "@mui/icons-material/Save";
import ArrowUpwardIcon from "@mui/icons-material/ArrowUpward";
import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import RestoreFromTrashIcon from "@mui/icons-material/RestoreFromTrash";
import DecisionPicker from "./DecisionPicker.tsx";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import Select from "@mui/material/Select";
import Backdrop from "@mui/material/Backdrop";
import CircularProgress from "@mui/material/CircularProgress";
import useFeedFilterStore from "./state.ts";
import { testFeed } from "./api.ts";
import ShowXmlDialog from "./ShowXmlDialog.tsx";

export interface EditPageProps {
  params?: {
    feedId?: string;
  };
}

const actionCellWidth = "120px";

interface EditableRule extends Rule {
  id: number;
  deleted: boolean;
  new: boolean;
}

interface TestState {
  result: FeedFilteringResult | undefined;
  loading: boolean;
  xmlOpen: boolean;
  error: string | undefined;
}

const decisionColor = (decision: Decision): string | undefined => {
  switch (decision) {
    case "accept":
      return undefined;
    case "reject":
      return "error";
    case "promote":
      return "success";
    case "demote":
      return "warning";
  }
};

const decisionName = (decision: Decision): string => {
  switch (decision) {
    case "accept":
      return "Accept";
    case "reject":
      return "Reject";
    case "promote":
      return "Promote";
    case "demote":
      return "Demote";
  }
};

const newEmptyRule = (i: number): EditableRule => ({
  id: i,
  index: i + 1,
  deleted: false,
  new: true,
  field: "title",
  customXPath: undefined,
  testedAttributeName: undefined,
  testType: "exact",
  testExpression: "",
  decision: "reject",
  comment: undefined,
});

export default function EditPage(props: EditPageProps): ReactElement {
  const token = useFeedFilterStore((store) => store.token);
  const feeds = useFeedFilterStore((store) => store.feeds);
  const save = useFeedFilterStore((store) => store.save);
  const loading = useFeedFilterStore((store) => store.loading);
  const [, navigate] = useLocation();

  const isEditMode = props.params?.feedId !== undefined;
  const editedFeed = isEditMode ? feeds.find((f) => f.feedId === props.params?.feedId) : undefined;

  const [feedId, setFeedId] = useState<string>(editedFeed?.feedId ?? "");
  const [description, setDescription] = useState<string>(editedFeed?.description ?? "");
  const [url, setUrl] = useState<string>(editedFeed?.url ?? "");
  const [defaultDecision, setDefaultDecision] = useState<Decision>(
    editedFeed?.defaultDecision ?? "accept",
  );

  const [rules, setRules] = useState<EditableRule[]>(
    editedFeed != undefined
      ? [
          ...editedFeed.rules.map((x, i) => ({
            ...x,
            index: i + 1,
            id: i,
            deleted: false,
            new: false,
          })),
          newEmptyRule(editedFeed.rules.length),
        ]
      : [newEmptyRule(0)],
  );

  const [testState, setTestState] = useState<TestState>({
    result: undefined,
    loading: false,
    error: undefined,
    xmlOpen: false,
  });
  const [saveError, setSaveError] = useState<string | undefined>(undefined);

  const moveUp = useCallback(
    (ruleId: number) =>
      setRules((rr: EditableRule[]): EditableRule[] => {
        const thisRuleIndex = rr.findIndex((x) => x.id === ruleId);
        if (thisRuleIndex === -1 || thisRuleIndex === 0) return rr;
        const thisRule = rr[thisRuleIndex];
        const aboveRule = rr[thisRuleIndex - 1];
        const newRules = [...rr];
        newRules[thisRuleIndex - 1] = { ...thisRule, index: aboveRule.index };
        newRules[thisRuleIndex] = { ...aboveRule, index: thisRule.index };
        return newRules;
      }),
    [],
  );

  const moveDown = useCallback(
    (ruleId: number) =>
      setRules((rr: EditableRule[]): EditableRule[] => {
        const thisRuleIndex = rr.findIndex((x) => x.id === ruleId);
        if (thisRuleIndex === -1 || thisRuleIndex === rr.length - 1) return rr;
        const thisRule = rr[thisRuleIndex];
        const belowRule = rr[thisRuleIndex + 1];
        const newRules = [...rr];
        newRules[thisRuleIndex + 1] = { ...thisRule, index: belowRule.index };
        newRules[thisRuleIndex] = { ...belowRule, index: thisRule.index };
        return newRules;
      }),
    [],
  );

  const updateRule = useCallback(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (ruleId: number, update: any) =>
      setRules((rr) => {
        const updatedRules: EditableRule[] = [
          ...rr.map((r) => (r.id == ruleId ? { ...r, ...update, new: false } : r)),
        ];

        if (!updatedRules.some((ur) => ur.new)) {
          updatedRules.push(newEmptyRule(updatedRules.length));
        }

        return updatedRules;
      }),
    [],
  );

  const buildFeedUpdate = useCallback(
    (cleanUpIndexes: boolean) => {
      const newRules = rules
        .filter((r) => !r.deleted && !r.new)
        .map((r, i) => {
          // eslint-disable-next-line @typescript-eslint/no-unused-vars
          const { id: _1, deleted: _2, new: _3, ...rest } = r;
          const newRule: Rule = { ...rest, index: cleanUpIndexes ? (i + 1) * 10 : rest.index };
          return newRule;
        });

      const update: FeedUpdate = { description, url, defaultDecision, rules: newRules };
      return update;
    },
    [description, url, defaultDecision, rules],
  );

  const doSave = useCallback(async () => {
    setSaveError(undefined);
    try {
      await save(feedId, buildFeedUpdate(true));
      navigate("/_admin");
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (e: any) {
      setSaveError(e.toString());
    }
  }, [feedId, buildFeedUpdate, save, navigate]);

  const runTest = useCallback(async () => {
    setTestState((s) => ({ ...s, loading: true, error: undefined }));
    try {
      const feedUpdate = buildFeedUpdate(false);
      const result = await testFeed(token!, feedUpdate);
      setTestState((s) => ({ ...s, result, loading: false, error: undefined }));
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (e: any) {
      setTestState((s) => ({ ...s, result: undefined, loading: false, error: e.toString() }));
    } finally {
      setTestState((s) => ({ ...s, loading: false }));
    }
  }, [token, buildFeedUpdate]);

  if (isEditMode && !editedFeed) {
    return (
      <>
        <PageHeader title="Error" showBackButton={true} />
        <Alert severity="error" sx={{ mt: 2 }}>
          Feed with ID "{props.params?.feedId}" not found.
        </Alert>
      </>
    );
  }

  return (
    <>
      <PageHeader title={!isEditMode ? "Create Feed" : "Edit Feed"} showBackButton={true} />
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={6}>
          <Paper sx={{ padding: 2, height: "100%", display: "flex" }}>
            <Grid container spacing={2}>
              <FormControl fullWidth>
                <FormLabel htmlFor="feedId">Feed ID</FormLabel>
                <OutlinedInput
                  fullWidth
                  id="feedId"
                  readOnly={isEditMode}
                  sx={{ fontFamily: "Monospace" }}
                  value={feedId}
                  onChange={(e) => setFeedId(e.target.value)}
                />
              </FormControl>

              <FormControl fullWidth>
                <FormLabel htmlFor="feedUrl">Feed URL</FormLabel>
                <OutlinedInput
                  id="feedUrl"
                  value={url}
                  sx={{ fontFamily: "Monospace" }}
                  onChange={(e) => setUrl(e.target.value)}
                />
              </FormControl>

              <FormControl fullWidth>
                <FormLabel htmlFor="feedDescription">Description</FormLabel>
                <OutlinedInput
                  id="feedDescription"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </FormControl>

              <FormControl fullWidth>
                <FormLabel htmlFor="defaultDecision">Default Decision</FormLabel>
                <DecisionPicker
                  value={defaultDecision}
                  setValue={setDefaultDecision}
                  id="defaultDecision"
                />
              </FormControl>
            </Grid>
          </Paper>
        </Grid>
        <Grid size={6}>
          <Paper sx={{ height: "100%", display: "flex" }}>
            <Box>
              <Box sx={{ padding: 2 }}>
                <Box>
                  <Button
                    variant="outlined"
                    loading={testState.loading}
                    onClick={() => runTest()}
                    sx={{ mr: 1 }}>
                    Test Feed
                  </Button>
                  <Button
                    variant="text"
                    disabled={testState.loading || testState.result == undefined}
                    onClick={() => setTestState((s) => ({ ...s, xmlOpen: true }))}>
                    Show XML
                  </Button>
                  {testState.result != undefined && (
                    <ShowXmlDialog
                      feedId={feedId}
                      originalXml={testState.result!.originalXml}
                      filteredXml={testState.result!.filteredXml}
                      open={testState.xmlOpen}
                      setOpen={(xmlOpen) => setTestState((s) => ({ ...s, xmlOpen }))}
                    />
                  )}
                </Box>
                {testState.result == undefined && testState.error == undefined && (
                  <Box>
                    To test the rules configured below, press <strong>Test Feed</strong>. The titles
                    of posts will be displayed alongside their decisions and IDs of rules that
                    caused the decision.
                  </Box>
                )}
                {testState.error != undefined && <Alert severity="error">{testState.error}</Alert>}
              </Box>
              {testState.result != undefined && (
                <TableContainer component={Box} maxHeight={320}>
                  <Table sx={{ maxHeight: 400 }} size="small" aria-label="test table">
                    <TableHead>
                      <TableRow sx={{ th: { fontWeight: 600 } }}>
                        <TableCell>Title</TableCell>
                        <TableCell>Tested Values</TableCell>
                        <TableCell>Decision</TableCell>
                        <TableCell>Rule&nbsp;#</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {testState.result.entryResults.map((result, i) => (
                        <TableRow
                          key={(result.entryTitle ?? "") + i}
                          sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                          <TableCell component="th" scope="row">
                            {result.entryTitle}
                          </TableCell>
                          <TableCell>
                            <Typography sx={{ fontSize: "smaller", whiteSpace: "pre-wrap" }}>
                              {result.testedValues != undefined
                                ? "'" +
                                  result.testedValues.join("', '").replace("\n", "\\n") +
                                  "'"
                                : "—"}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography
                              sx={{ fontSize: "inherit" }}
                              color={decisionColor(result.decision)}>
                              {decisionName(result.decision)}
                            </Typography>
                          </TableCell>
                          <TableCell>{result.decidingRule?.index ?? "—"}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </Box>
          </Paper>
        </Grid>
      </Grid>

      <TableContainer component={Paper}>
        <Table sx={{ minWidth: 650, tableLayout: "fixed" }} aria-label="rule table">
          <TableHead>
            <TableRow sx={{ th: { fontWeight: 600 } }}>
              <TableCell sx={{ width: "3%" }}>#</TableCell>
              <TableCell>Field</TableCell>
              <TableCell>Test Type</TableCell>
              <TableCell>Test Expression</TableCell>
              <TableCell>Decision</TableCell>
              <TableCell>Comment</TableCell>
              <TableCell sx={{ width: actionCellWidth }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rules.map((rule) => (
              <TableRow
                key={rule.id}
                sx={{
                  "&:last-child td, &:last-child th": { border: 0 },
                  background: rule.deleted
                    ? "rgba(255, 0, 0, 0.1)"
                    : rule.new
                      ? "rgba(0, 128, 0, 0.1)"
                      : undefined,
                }}>
                <TableCell component="th" scope="row">
                  {rule.new ? "+" : rule.index}
                </TableCell>
                <TableCell>
                  <Select
                    fullWidth
                    value={rule.field}
                    onChange={(e) => updateRule(rule.id, { field: e.target.value })}
                    disabled={rule.deleted}>
                    <MenuItem value="title">Title</MenuItem>
                    <MenuItem value="author">Author</MenuItem>
                    <MenuItem value="link">Link</MenuItem>
                    <MenuItem value="category">Category</MenuItem>
                    <MenuItem value="content">Content</MenuItem>
                    <MenuItem value="custom">Custom</MenuItem>
                  </Select>
                  {rule.field === "custom" && (
                    <>
                      <TextField
                        fullWidth
                        variant="outlined"
                        label="Field XPath"
                        value={rule.customXPath ?? ""}
                        onChange={(e) =>
                          updateRule(rule.id, {
                            customXPath: e.target.value === "" ? undefined : e.target.value,
                          })
                        }
                        size="small"
                        sx={{ mt: 1 }}
                        disabled={rule.deleted}
                      />
                      <TextField
                        fullWidth
                        variant="outlined"
                        label="Attribute (optional)"
                        value={rule.testedAttributeName ?? ""}
                        onChange={(e) =>
                          updateRule(rule.id, {
                            testedAttributeName: e.target.value === "" ? undefined : e.target.value,
                          })
                        }
                        size="small"
                        sx={{ mt: 1 }}
                        disabled={rule.deleted}
                      />
                    </>
                  )}
                </TableCell>
                <TableCell>
                  <Select
                    fullWidth
                    value={rule.testType}
                    onChange={(e) => updateRule(rule.id, { testType: e.target.value })}
                    disabled={rule.deleted}>
                    <MenuItem value="exact">Equals</MenuItem>
                    <MenuItem value="contains">Contains</MenuItem>
                    <MenuItem value="startsWith">Starts With</MenuItem>
                    <MenuItem value="endsWith">Ends With</MenuItem>
                    <MenuItem value="regex">Regex</MenuItem>
                  </Select>
                </TableCell>
                <TableCell>
                  <OutlinedInput
                    fullWidth
                    value={rule.testExpression}
                    onChange={(e) => updateRule(rule.id, { testExpression: e.target.value })}
                    disabled={rule.deleted}
                  />
                </TableCell>
                <TableCell>
                  <DecisionPicker
                    value={rule.decision}
                    setValue={(d) => updateRule(rule.id, { decision: d })}
                    disabled={rule.deleted}
                  />
                </TableCell>
                <TableCell>
                  <OutlinedInput
                    fullWidth
                    value={rule.comment ?? ""}
                    onChange={(e) =>
                      updateRule(rule.id, {
                        comment: e.target.value === "" ? undefined : e.target.value,
                      })
                    }
                    disabled={rule.deleted}
                  />
                </TableCell>
                <TableCell sx={{ width: actionCellWidth }} align="right">
                  <Tooltip title="Move Up">
                    <span>
                      <IconButton
                        aria-label="up"
                        size="small"
                        disabled={rule.deleted || rule.new || rule.index === 1}
                        onClick={() => moveUp(rule.id)}>
                        <ArrowUpwardIcon fontSize="inherit" />
                      </IconButton>
                    </span>
                  </Tooltip>

                  <Tooltip title="Move Down">
                    <span>
                      <IconButton
                        aria-label="down"
                        size="small"
                        disabled={rule.deleted || rule.new || rule.index === rules.length}
                        onClick={() => moveDown(rule.id)}>
                        <ArrowDownwardIcon fontSize="inherit" />
                      </IconButton>
                    </span>
                  </Tooltip>

                  <Tooltip title={rule.deleted ? "Restore" : "Delete"}>
                    <span>
                      <IconButton
                        aria-label="delete"
                        size="small"
                        onClick={() => updateRule(rule.id, { deleted: !rule.deleted })}
                        disabled={rule.new}>
                        {rule.deleted ? (
                          <RestoreFromTrashIcon fontSize="inherit" />
                        ) : (
                          <DeleteIcon fontSize="inherit" />
                        )}
                      </IconButton>
                    </span>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {saveError && (
        <Alert color="error" sx={{ mt: 2, mb: 2 }}>
          {saveError}
        </Alert>
      )}

      <Grid container sx={{ mt: 2, mb: 2 }}>
        <Grid display="flex" justifyContent="center" alignItems="center" size="grow">
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            size="large"
            onClick={() => doSave()}>
            Save
          </Button>
        </Grid>
      </Grid>

      <Backdrop open={loading}>
        <CircularProgress color="inherit" />
      </Backdrop>
    </>
  );
}
