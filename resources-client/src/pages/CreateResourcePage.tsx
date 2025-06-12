import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Box,
  Button,
  Container,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  TextField,
  Typography,
  Alert,
} from '@mui/material';
import { createResource, getCompetencies } from '@/services/api';

function CreateResourcePage() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [yearsOfExperience, setYearsOfExperience] = useState('');
  const [selectedCompetencies, setSelectedCompetencies] = useState<number[]>([]);
  const [error, setError] = useState('');

  const { data: competencies = [] } = useQuery({
    queryKey: ['competencies'],
    queryFn: getCompetencies,
  });

  const createMutation = useMutation({
    mutationFn: createResource,
    onSuccess: () => {
      navigate('/resources');
    },
    onError: (error: Error) => {
      setError(error.message);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name || !yearsOfExperience || selectedCompetencies.length === 0) {
      setError('Por favor, preencha todos os campos obrigatórios');
      return;
    }

    createMutation.mutate({
      name,
      birthDate: birthDate ? birthDate : undefined,
      yearsOfExperience: parseInt(yearsOfExperience),
      competencyIds: selectedCompetencies,
    });
  };

  const handleCompetencyChange = (event: SelectChangeEvent<number[]>) => {
    setSelectedCompetencies(event.target.value as number[]);
  };

  return (
    <Container maxWidth="sm">
      <Paper elevation={3} sx={{ p: 4, mt: 4 }}>
        <Typography variant="h5" component="h1" gutterBottom>
          Criar Novo Recurso
        </Typography>

        <Box component="form" onSubmit={handleSubmit}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <TextField
            fullWidth
            required
            label="Nome"
            value={name}
            onChange={(e) => setName(e.target.value)}
            margin="normal"
          />

          <TextField
            fullWidth
            label="Data de Nascimento"
            type="date"
            value={birthDate}
            onChange={(e) => setBirthDate(e.target.value)}
            margin="normal"
            InputLabelProps={{
              shrink: true,
            }}
          />

          <TextField
            fullWidth
            required
            label="Anos de Experiência"
            type="number"
            value={yearsOfExperience}
            onChange={(e) => setYearsOfExperience(e.target.value)}
            margin="normal"
            inputProps={{ min: 0 }}
          />

          <FormControl fullWidth margin="normal" required>
            <InputLabel>Competências</InputLabel>
            <Select
              multiple
              value={selectedCompetencies}
              onChange={handleCompetencyChange}
              label="Competências"
            >
              {competencies.map((competency) => (
                <MenuItem key={competency.id} value={competency.id}>
                  {competency.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Button
              type="button"
              variant="outlined"
              onClick={() => navigate('/resources')}
              fullWidth
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="contained"
              fullWidth
              disabled={createMutation.isPending}
            >
              {createMutation.isPending ? 'Criando...' : 'Criar'}
            </Button>
          </Box>
        </Box>
      </Paper>
    </Container>
  );
}

export default CreateResourcePage; 